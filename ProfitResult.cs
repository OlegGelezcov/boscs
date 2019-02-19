namespace Bos {
    public class ProfitResult {

        public double ValuePerRound { get; private set; }
        public double ValuePerSecond { get; private set; }
        public double GenerationInterval { get; private set; }

        public ProfitResult(double valPerRound, double valPerSecond, double generationInterval) {
            ValuePerRound = valPerRound;
            ValuePerSecond = valPerSecond;
            GenerationInterval = generationInterval;
        }

        public void UpdateResult(double valuePerRound, double valuePerSecond, double generationInterval) {
            this.ValuePerRound = valuePerRound;
            this.ValuePerSecond = valuePerSecond;
            this.GenerationInterval = generationInterval;
        }

        public void UpdateFromOther(ProfitResult other) {
            this.ValuePerRound = other.ValuePerRound;
            this.ValuePerSecond = other.ValuePerSecond;
            this.GenerationInterval = other.GenerationInterval;
        }
    }

}