﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using ProtoBuf;
using System.IO;
using System.Linq;
using ProtoBuf.Meta;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.DataSource;
using QuantConnect.Orders;

namespace QuantConnect.DataLibrary.Tests
{
    [TestFixture]
    public class QuiverCongressTests
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        
        [Test]
        public void DeserializeRawQuiverCongressJson()
        {
            // This information is not factual and is only used for testing purposes
            var content = "{ \"ReportDate\": \"2020-01-01\", \"TransactionDate\": \"2019-12-23\", \"Representative\": \"Cory Gardner\", \"Transaction\": \"Purchase\", \"Amount\": 100, \"House\": \"Senate\" }";
            var data = JsonConvert.DeserializeObject<QuiverCongress>(content, _jsonSerializerSettings);

            Assert.AreEqual(new DateTime(2020, 1, 1), data.ReportDate);
            Assert.AreEqual(new DateTime(2019, 12, 23), data.TransactionDate);
            Assert.AreEqual("Cory Gardner", data.Representative);
            Assert.AreEqual(OrderDirection.Buy, data.Transaction);
            Assert.AreEqual(100m, data.Amount);
            Assert.AreEqual(Congress.Senate, data.House);
        }
        
        [Test]
        public void JsonRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();
            var serialized = JsonConvert.SerializeObject(expected);
            var result = JsonConvert.DeserializeObject(serialized, type);

            AssertAreEqual(expected, result);
        }

        [Test]
        public void ProtobufRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();

            RuntimeTypeModel.Default[typeof(BaseData)].AddSubType(2000, type);

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, expected);

                stream.Position = 0;

                var result = Serializer.Deserialize(type, stream);

                AssertAreEqual(expected, result, filterByCustomAttributes: true);
            }
        }

        [Test]
        public void Clone()
        {
            var expected = CreateNewInstance();
            var result = expected.Clone();

            AssertAreEqual(expected, result);
        }

        private void AssertAreEqual(object expected, object result, bool filterByCustomAttributes = false)
        {
            foreach (var propertyInfo in expected.GetType().GetProperties())
            {
                // we skip Symbol which isn't protobuffed
                if (filterByCustomAttributes && propertyInfo.CustomAttributes.Count() != 0)
                {
                    Assert.AreEqual(propertyInfo.GetValue(expected), propertyInfo.GetValue(result));
                }
            }
            foreach (var fieldInfo in expected.GetType().GetFields())
            {
                Assert.AreEqual(fieldInfo.GetValue(expected), fieldInfo.GetValue(result));
            }
        }

        private BaseData CreateNewInstance()
        {
            return new QuiverCongress
            {
                Symbol = Symbol.Empty,
                Time = DateTime.Today,
                DataType = MarketDataType.Base,
                
                ReportDate = DateTime.Today,
                TransactionDate = DateTime.Today.AddDays(-60),
                Representative = "Ronald Lee Wyden",
                Transaction = OrderDirection.Buy,
                
                Amount = 15001m,
                House = Congress.Senate,
            };
        }
    }
}
