using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using RestSharp.Deserializers;

namespace Answer_Factory_Unit_Tests
{
    using AnswerFactory;

    [TestClass]
    public class ObjectDeserialization
    {
        private static JsonDeserializer deserial = new JsonDeserializer();
        [TestMethod]
        public void Project2Deserialization()
        {
            //try
            //{
                string content = TestData.Project2JsonData;
                var result =
                    deserial.Deserialize<Project2>(new RestResponse<Project2> { Content = content });

                if (result == null)
                {
                    Assert.Fail();
                }
            //}
            //catch (Exception error)
            //{
            //    Assert.Fail();
            //}
        }
    }
}
