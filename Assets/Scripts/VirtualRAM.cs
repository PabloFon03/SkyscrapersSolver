using System.Collections.Generic;

public static class VirtualRAM
{
    [System.Serializable]
    public struct GridRow
    {
        public int[] row;
        public int leftSum;
        public int rightSum;
    }
    public static HashSet<GridRow>[] validRows;
    public struct GridData
    {
        public int size;
        public int[][] edgeNums;
        public int[][] filledSlots;
    };
    public static GridData gridData;
}