using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    public class CsvLineParserTests
    {
        [Fact]
        public void Parse_SimpleLine()
        {
            var fields = CsvLineParser.Parse("3001,Чехол-книжка,Чехол с застежкой,990,RUB,Чехлы,25,/images/t.jpg,true,M,белый,силикон");

            Assert.Equal(12, fields.Count);
            Assert.Equal("3001", fields[0]);
        }

        [Fact]
        public void Parse_EmptyFields()
        {
            var fields = CsvLineParser.Parse("3011,Чехол для ноутбука,,590,RUB,Чехлы,40,,true,13,белый,");

            Assert.Equal(12, fields.Count);
            Assert.Equal("", fields[2]);
            Assert.Equal("", fields[7]);
        }

        [Fact]
        public void Parse_CommaInsideQuotes()
        {
            var fields = CsvLineParser.Parse("3002,\"Чехол, противоударный\",Описание,1490,RUB,Чехлы,1,,true,L,чёрный,силикон");

            Assert.Equal(12, fields.Count);
            Assert.Equal("Чехол, противоударный", fields[1]);
        }

        [Fact]
        public void Parse_EscapedQuotes()
        {
            var fields = CsvLineParser.Parse("3013,\"Платье \"\"Special\"\"\",Описание");

            Assert.Equal("Платье \"Special\"", fields[1]);
        }
    }
}
