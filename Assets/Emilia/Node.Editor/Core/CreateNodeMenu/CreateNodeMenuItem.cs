namespace Emilia.Node.Editor
{
    public class CreateNodeMenuItem
    {
        public CreateNodeMenuItem parent;
        
        public CreateNodeInfo info;
        public string title;
        public int level;

        public CreateNodeMenuItem() { }

        public CreateNodeMenuItem(CreateNodeInfo info, string title, int level)
        {
            this.info = info;
            this.title = title;
            this.level = level;
        }
    }
}