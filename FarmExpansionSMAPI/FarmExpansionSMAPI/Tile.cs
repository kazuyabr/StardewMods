namespace FarmExpansionSMAPI
{
    public class Tile
    {
        public int l;
        public int x;
        public int y;
        public int tileIndex;
        public string layer;
        public int tileSheet = 1;

        public Tile(int l, int x, int y, int tileIndex)
        {
            this.l = l; this.x = x; this.y = y; this.tileIndex = tileIndex;
            setLayerName(l);
        }

        public Tile(int l, int x, int y, int tileIndex, int tileSheet)
        {
            this.l = l; this.x = x; this.y = y; this.tileIndex = tileIndex; this.tileSheet = tileSheet;
            setLayerName(l);
        }

        private void setLayerName(int l)
        {
            switch (l)
            {
                case 0:
                    layer = "Back";
                    break;
                case 1:
                    layer = "Buildings";
                    break;
                case 2:
                    layer = "Paths";
                    break;
                case 3:
                    layer = "Front";
                    break;
                case 4:
                    layer = "AlwaysFront";
                    break;
                default:
                    goto case 0;
            }
        }
    }
}
