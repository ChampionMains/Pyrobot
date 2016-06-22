using System;
using System.Net;
using System.Runtime.Serialization;

namespace ChampionMains.Pyrobot.Services.Riot
{
    [Serializable]
    public class RiotHttpException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public RiotHttpException(HttpStatusCode statusCode)
        {
        }

        public RiotHttpException(string message) : base(message)
        {
        }

        public RiotHttpException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RiotHttpException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            StatusCode = (HttpStatusCode) info.GetInt32("StatusCode");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("StatusCode", (int) StatusCode);
        }
    }
}