using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class PropertyGridEditableProperties : ICustomTypeDescriptor
    {
        private readonly IDictionary<string, object> _properties;

        public PropertyGridEditableProperties(ExpandoObject expando)
        {
            _properties = expando as IDictionary<string, object>;
        }

        // 实现 ICustomTypeDescriptor 接口的必要方法
        public PropertyDescriptorCollection GetProperties()
        {
            var properties = new List<PropertyDescriptor>();

            foreach (var kvp in _properties)
            {
                properties.Add(new DynamicPropertyDescriptor(kvp.Key, kvp.Value?.GetType() ?? typeof(object)));
            }

            return new PropertyDescriptorCollection(properties.ToArray());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        // 其他 ICustomTypeDescriptor 接口方法的默认实现
        public AttributeCollection GetAttributes() => AttributeCollection.Empty;
        public string GetClassName() => null;
        public string GetComponentName() => null;
        public TypeConverter GetConverter() => null;
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => null;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _properties;
        }

    }

    // 动态属性描述器
    public class DynamicPropertyDescriptor : PropertyDescriptor
    {
        private readonly string _propertyName;
        private readonly Type _propertyType;

        public DynamicPropertyDescriptor(string propertyName, Type propertyType)
            : base(propertyName, null)
        {
            _propertyName = propertyName;
            _propertyType = propertyType;
        }

        public override bool CanResetValue(object component) => false;
        public override Type ComponentType => typeof(PropertyGridEditableProperties);
        public override string DisplayName => _propertyName;
        public override string Description => _propertyName;
        public override bool IsReadOnly => false;
        public override Type PropertyType => _propertyType;

        public override object GetValue(object component)
        {
            var dictionary = ((PropertyGridEditableProperties)component)
                .GetPropertyOwner(this) as IDictionary<string, object>;
            return dictionary[_propertyName];
        }

        public override void ResetValue(object component) { }

        public override void SetValue(object component, object value)
        {
            var dictionary = ((PropertyGridEditableProperties)component)
                .GetPropertyOwner(this) as IDictionary<string, object>;
            if (dictionary == null)
            {
                return;
            }
            dictionary[_propertyName] = value;
        }

        public override bool ShouldSerializeValue(object component) => true;
    }
}
