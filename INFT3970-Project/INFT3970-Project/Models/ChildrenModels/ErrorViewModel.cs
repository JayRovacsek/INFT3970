using System;

namespace INFT3970Project.Models
{
    public class ErrorViewModel : MasterModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}