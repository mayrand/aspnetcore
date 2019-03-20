using Microsoft.AspNetCore.Builder;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ThrottlingGateway.Models;

namespace ThrottlingGateway.Middleware
{
    public static class ThrottlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseThrottlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThrottlingMiddleware>();
        }

        public static LoadInfoResponse FromXml(this string xml)
        {
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = new XmlTextReader(stringReader))
            {
                var ser = new XmlSerializer(typeof(LoadInfoResponse));
                var obj = ser.Deserialize(xmlReader) as LoadInfoResponse;
                obj.Occured = DateTime.Now;
                xmlReader.Close();
                stringReader.Close();
                return obj;
            }
        }
    }
}