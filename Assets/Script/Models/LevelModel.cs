using Tuleeeeee.Data;
using Tuleeeeee.Enum;

namespace Tuleeeeee.Model
{
    public class LevelModel
    {
        public GemMap[,] CloneTable;

        public int CrushCount;

        public bool HasClear()
        {
            for (int i = 0; i < CloneTable.GetLength(0); i++)
            {
                for (int j = 0; j < CloneTable.GetLength(1); j++)
                {
                    if (CloneTable[i, j].gemType != GemType.None)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}