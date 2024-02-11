using ParagliderFlightLog.Models;

namespace ParagliderFlightLogTest
{
    [TestClass]
    public class FlightTest
    {
        [TestMethod]
        public void TakeOffDateTime_CorrectIGC__ReturnsDateTime()
        {
            //Arrange
            var flight = new FlightWithData();
            string TakeOffTimeIGC = "HFDTEDATE:010203,01\r\nB0405060000000N00000000EA000000000000\r\n"; // 01/02/03 at 04:05:06
            flight.IgcFileContent = TakeOffTimeIGC;
            var ExpectedDateTime = new DateTime(2003,2,1,4,5,6, DateTimeKind.Utc);
            //Act
            DateTime actualDateTime = flight.TakeOffDateTime;
            //Assert
            Assert.AreEqual(ExpectedDateTime, actualDateTime);
        }
        [TestMethod]
        public void TakeOffDateTime_NoIGC_StoredBackField_ReturnBackField()
        {
            //Arrange
            var flight = new Flight();
            var expectedDateTime = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
            flight.TakeOffDateTime = expectedDateTime;
            //Act
            DateTime actualDateTime = flight.TakeOffDateTime;
            //Assert
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }
        [TestMethod]
        public void TakeOffDateTime_NoIGC_NoBackField_ReturnNull()
        {
            //Arrange
            var flight = new Flight();
            DateTime expectedDateTime = DateTime.MinValue;
            //Act
            DateTime actualDateTime = flight.TakeOffDateTime;
            //Assert
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }
    }
}