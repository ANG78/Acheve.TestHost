﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace Microsoft.AspNetCore.TestHost
{
    /// <summary>
    /// An implementation of <see cref="RequestContentOptions"/> that includes 
    /// the [FromForm] parameter as <see cref="IDictionary<>"/>
    /// </summary>
    public class IncludeContentAsFormUrlEncoded : RequestContentOptions
    {
        /// <inheritdoc/>
        public override bool IncludeFromBodyAsContent => false;
        
        /// <inheritdoc/>
        public override bool IncludeFromFormAsContent => true;

        /// <inheritdoc/>
        public override Func<object, HttpContent> ContentBuilder =>
            content => new FormUrlEncodedContent(ToKeyValue(content));

        private IDictionary<string, string> ToKeyValue(object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            if (!(metaToken is JToken token))
            {
                return ToKeyValue(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                                                 .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                            jValue?.ToString("o", CultureInfo.InvariantCulture) :
                            jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }
    }
}
