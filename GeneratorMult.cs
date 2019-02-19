namespace Bos {
    public class GeneratorMult {
        public string Id { get; private set; }
        public double Mult { get; private set; }

        public GeneratorMult(string id, double mult) {
            Id = id;
            Mult = mult;
        }

        public override string ToString() {
            return $"Id: {Id}, Mult: {Mult}";
        }
    }

}