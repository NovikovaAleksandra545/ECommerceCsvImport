using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    public class CsvLineParserTests
    {
        [Fact]
        public void Parse_SimpleLine()
        {
            var fields = CsvLineParser.Parse("3001,Футболка белая,Летняя футболка,990,RUB,Футболки,25,/images/t.jpg,true,M,белый,хлопок");

            Assert.Equal(12, fields.Count);
            Assert.Equal("3001", fields[0]);
        }

        [Fact]
        public void Parse_EmptyFields()
        {
            var fields = CsvLineParser.Parse("3011,Носки,,590,RUB,Аксессуары,40,,true,42,белый,");

            Assert.Equal(12, fields.Count);
            Assert.Equal("", fields[2]);
            Assert.Equal("", fields[7]);
        }

        [Fact]
        public void Parse_CommaInsideQuotes()
        {
            var fields = CsvLineParser.Parse("3002,\"Футболка, оверсайз\",Описание,1490,RUB,Футболки,1,,true,L,чёрный,хлопок");

            Assert.Equal(12, fields.Count);
            Assert.Equal("Футболка, оверсайз", fields[1]);
        }

        [Fact]
        public void Parse_EscapedQuotes()
        {
            var fields = CsvLineParser.Parse("3013,\"Платье \"\"Special\"\"\",Описание");

            Assert.Equal("Платье \"Special\"", fields[1]);
        }
    }
}
