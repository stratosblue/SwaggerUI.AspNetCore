namespace SwaggerUI.AspNetCore.Test;

[TestClass]
public class SwaggerUIOptionsTests
{
    #region Public 方法

    [TestMethod]
    [DataRow("0")]
    [DataRow("x")]
    [DataRow("[]")]
    [DataRow("[{}]")]
    [DataRow("{x}")]
    [DataRow("Null")]
    [DataRow("{property:\"value\"}")]
    [DataRow("{\"property\":value}")]
    public void Should_Set_CustomConfigurationObject_Fail(string value)
    {
        var options = new SwaggerUIOptions();
        Assert.ThrowsExactly<ArgumentException>(() => options.CustomConfigurationObject = value);
        Assert.IsNull(options.CustomConfigurationObject);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("{}")]
    [DataRow("{//Comment\n}")]
    [DataRow("null")]
    [DataRow("{\"property\":\"value\"}")]
    [DataRow("//Comment\n{//Comment\n\"property\"://Comment\n\"value\"}")]
    public void Should_Set_CustomConfigurationObject_Success(string value)
    {
        var options = new SwaggerUIOptions
        {
            CustomConfigurationObject = value
        };

        if (string.IsNullOrWhiteSpace(value))
        {
            Assert.IsNull(options.CustomConfigurationObject);
        }
        else
        {
            Assert.AreEqual(value, options.CustomConfigurationObject);
        }
    }

    #endregion Public 方法
}
