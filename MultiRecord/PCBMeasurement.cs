using System;

namespace MultiRecord
{
    public class PCBMeasurement
    {
        public string Id { get; set; }
        public string MeasurementSessionId { get; set; }
        public string MarkerId { get; set; }
        public int MeasurementNumber { get; set; }
        public decimal? MeasuredValue { get; set; }
        public string ToleranceStatus { get; set; }
        public decimal ToleranceUpper { get; set; }
        public decimal ToleranceLower { get; set; }
        public string ToleranceUpperType { get; set; }
        public string ToleranceLowerType { get; set; }
        public bool ToleranceEnabled { get; set; }
        public string MarkerType { get; set; }
        public string MarkerParameters { get; set; }
        public decimal MarkerPositionX { get; set; }
        public decimal MarkerPositionY { get; set; }
        public string MarkerDisplayName { get; set; }
        public string MarkerColor { get; set; }
        public string Notes { get; set; }
        public string MeasurementUnit { get; set; }
        public decimal? ToleranceUpperLimit { get; set; }
        public decimal? ToleranceLowerLimit { get; set; }
        public bool? Open { get; set; }
    }
}
