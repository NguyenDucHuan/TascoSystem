using AutoMapper;
using System;

namespace Tasco.TaskService.API.Mapping
{
    public class GuidValueResolver<TSource> : IValueResolver<TSource, object, Guid>
    {
        private readonly Func<TSource, string> _getValue;
        private readonly string _fieldName;

        public GuidValueResolver(Func<TSource, string> getValue, string fieldName)
        {
            _getValue = getValue;
            _fieldName = fieldName;
        }

        public Guid Resolve(TSource source, object destination, Guid destMember, ResolutionContext context)
        {
            var value = _getValue(source);
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{_fieldName} cannot be null or empty");
            if (!Guid.TryParse(value, out var guid))
                throw new ArgumentException($"Invalid GUID format for {_fieldName}: {value}");
            return guid;
        }
    }
} 