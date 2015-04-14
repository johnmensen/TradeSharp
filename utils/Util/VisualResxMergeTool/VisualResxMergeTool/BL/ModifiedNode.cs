namespace VisualResxMergeTool.BL
{
    public class ModifiedNode
    {
        public string Key { get; set; }

        public string ValueOwn { get; set; }

        public string ValueTheirs { get; set; }

        public ChangeSource UseFrom { get; set; }
    }
}
