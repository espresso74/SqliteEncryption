using Demo.Encrypt;
using DemoProject.Contract;
using SQLite;

namespace DemoProject.Entity
{
    public class Configuration : IAuditable, IEncryptable
    {
        [PrimaryKey]
        public string Name { get; set; }
        public string Value { get; set; }
        [Encrypted]
        public string Password { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
