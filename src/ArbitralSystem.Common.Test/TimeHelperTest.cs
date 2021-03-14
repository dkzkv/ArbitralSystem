using ArbitralSystem.Common.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Common.Test
{
    [TestClass]
    public class TimeHelperTest
    {
        //http://i-leon.ru/tools/time

        [TestMethod]
        public void DateTimeToTimeStampMethod()
        {
            var unixTime = TimeHelper.DateTimeToTimeStamp(new System.DateTime(2000, 1, 1, 0, 0, 0), false);
            Assert.AreEqual(unixTime, 946684800);
        }

        [TestMethod]
        public void TimeStampToDateTime()
        {
            var targetDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
            var dateTime = TimeHelper.TimeStampToUtcDateTime(946684800, false);
            Assert.AreEqual(dateTime, targetDate);
        }
    }
}
