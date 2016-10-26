namespace Application.Models {
    public class DestinationModel {
        public string FirstItem { get; set; }
        public string LastItem { get; set; }

        public override string ToString() {
            return $"First Item: {FirstItem}, Last Item: {LastItem}";
        }
    }
}