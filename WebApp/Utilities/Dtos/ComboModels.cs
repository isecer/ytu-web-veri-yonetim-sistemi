using System;

namespace WebApp.Utilities.Dtos
{
    public class CmbBoolDatetimeDto
    {
        public bool? Value { get; set; }
        public DateTime? Caption { get; set; }
    }
    public class CmbBoolDto
    {
        public bool? Value { get; set; }
        public string Caption { get; set; }
    }
    public class CmbDoubleDto
    {
        public double? Value { get; set; }
        public string Caption { get; set; }
    }
    public class CmbIntDto
    {
        public int? Value { get; set; }
        public string Caption { get; set; }
    }
    public class CmbStringDto
    {
        public string Value { get; set; }
        public string Caption { get; set; }
    }
}