namespace Bos {
    public interface IGenerationContext {
        double Global { get; }
        double Permanent { get; }
        double X2 { get; }
        double Planet { get; }
        double GetEnhance(int generatorId);
        double GetTimeToProfitBonus(IGenerator generator);
        int Internal { get; }
        void SetInternal(int value);
    }

}