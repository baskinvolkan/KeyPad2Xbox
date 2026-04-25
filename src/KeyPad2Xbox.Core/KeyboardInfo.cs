namespace KeyPad2Xbox.Core
{
    public class KeyboardInfo
    {
        public int DeviceId { get; set; }
        public string HardwareId { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"device #{DeviceId} - {HardwareId}";
        }
    }
}
