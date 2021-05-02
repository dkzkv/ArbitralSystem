namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models.LookUps
{
    /// <summary>
    /// Basic look up representation
    /// </summary>
    public abstract class LookUpInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }
}