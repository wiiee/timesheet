namespace Entity.ValueType
{
    public class Score
    {
        //TimeSheet里面计算的分数
        public int TimeSheetValue { get; set; }

        //去除级别后的分数
        public int StandardValue { get; set; }

        //相对100分的分数
        public int Outcome { get; set; }

        //Manager给的分数
        public int ManagerValue { get; set; }
        public string ManagerComment { get; set; }

        //最后计算的结果分数
        public int Result { get; set; }

        public int Level { get; set; }
    }
}
