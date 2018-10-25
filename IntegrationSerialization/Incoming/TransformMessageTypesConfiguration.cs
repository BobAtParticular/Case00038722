using System;
using System.Collections.Generic;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus
{
    public static class TransformMessageTypesConfiguration
    {
        public static EndpointConfiguration AddMessageTypeTransformation(this EndpointConfiguration config,
            Type typeToTransform, Func<object, object> transform)
        {
            var settings = config.GetSettings();
            if (!settings.HasExplicitValue(TransformMessageTypes.TransformMessageTypesSettingsKeys))
            {
                settings.Set(TransformMessageTypes.TransformMessageTypesSettingsKeys, new Dictionary<Type, Func<object,object>>());
            }

            var transforms = settings.Get(TransformMessageTypes.TransformMessageTypesSettingsKeys) as Dictionary<Type, Func<object, object>>;

            transforms.Add(typeToTransform, transform);

            return config;
        }
    }
}
