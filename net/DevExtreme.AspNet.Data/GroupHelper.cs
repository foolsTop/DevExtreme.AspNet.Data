﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DevExtreme.AspNet.Data {

    class GroupHelper<T> {
        readonly static object NULL_KEY = new object();

        IEnumerable<T> _data;
        Accessor<T> _accessor;

        public GroupHelper(IEnumerable<T> data, Accessor<T> accessor) {
            _data = data;
            _accessor = accessor;
        }

        public IList<Group> Group(IEnumerable<GroupingInfo> groupInfo) {
            return Group(_data, groupInfo);
        }

        IList<Group> Group(IEnumerable<T> data, IEnumerable<GroupingInfo> groupInfo) {
            var groups = Group(data, groupInfo.First());

            if(groupInfo.Count() > 1) {
                groups = groups
                    .Select(g => new Group {
                        key = g.key,
                        items = Group(g.items.Cast<T>(), groupInfo.Skip(1))
                            .Cast<object>()
                            .ToArray()
                    })
                    .ToArray();
            }

            return groups;
        }


        IList<Group> Group(IEnumerable<T> data, GroupingInfo groupInfo) {
            var groupsIndex = new Dictionary<object, Group>();
            var groups = new List<Group>();

            foreach(var item in data) {
                var groupKey = GetKey(item, groupInfo);
                var groupIndexKey = groupKey ?? NULL_KEY;

                if(!groupsIndex.ContainsKey(groupIndexKey)) {
                    var newGroup = new Group { key = groupKey };
                    groupsIndex[groupIndexKey] = newGroup;
                    groups.Add(newGroup);
                }

                var group = groupsIndex[groupIndexKey];
                if(group.items == null)
                    group.items = new List<object>();
                group.items.Add(item);
            }

            return groups;
        }

        object GetKey(T obj, GroupingInfo groupInfo) {
            var memberValue = _accessor.Read(obj, groupInfo.Selector);

            var intervalString = groupInfo.GroupInterval;
            if(String.IsNullOrEmpty(intervalString))
                return memberValue;

            if(Char.IsDigit(intervalString[0])) {
                var number = Convert.ToDecimal(memberValue);
                var interval = Decimal.Parse(intervalString);
                return number - number % interval;
            }

            switch(intervalString) {
                case "year":
                    return Convert.ToDateTime(memberValue).Year;
                case "quarter":
                    return (int)Math.Ceiling(Convert.ToDateTime(memberValue).Month / 3.0);
                case "month":
                    return Convert.ToDateTime(memberValue).Month ;
                case "day":
                    return Convert.ToDateTime(memberValue).Day;
                case "dayOfWeek":
                    return (int)Convert.ToDateTime(memberValue).DayOfWeek;
                case "hour":
                    return Convert.ToDateTime(memberValue).Hour;
                case "minute":
                    return Convert.ToDateTime(memberValue).Minute;
                case "second":
                    return Convert.ToDateTime(memberValue).Second;
            }

            throw new NotSupportedException();
        }
    }

}