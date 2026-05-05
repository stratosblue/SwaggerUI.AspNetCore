using System.Text.Json;

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
        var exception = Assert.ThrowsExactly<ArgumentException>(() => options.CustomConfigurationObject = value);

        Assert.AreEqual(nameof(SwaggerUIOptions.CustomConfigurationObject), exception.ParamName);
        Assert.IsNull(options.CustomConfigurationObject);
    }

    [TestMethod]
    public void Should_Set_CustomConfigurationObject_Fail_WithInvalidOperationExceptionAsInnerException_WhenJsonIsNotObject()
    {
        var options = new SwaggerUIOptions();

        var exception = Assert.ThrowsExactly<ArgumentException>(() => options.CustomConfigurationObject = "[]");

        Assert.AreEqual(nameof(SwaggerUIOptions.CustomConfigurationObject), exception.ParamName);
        Assert.IsInstanceOfType<InvalidOperationException>(exception.InnerException);
    }

    [TestMethod]
    public void Should_Set_CustomConfigurationObject_Fail_WithJsonExceptionAsInnerException()
    {
        var options = new SwaggerUIOptions();

        var exception = Assert.ThrowsExactly<ArgumentException>(() => options.CustomConfigurationObject = "{\"property\":value}");

        Assert.AreEqual(nameof(SwaggerUIOptions.CustomConfigurationObject), exception.ParamName);
        Assert.IsInstanceOfType<JsonException>(exception.InnerException);
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
