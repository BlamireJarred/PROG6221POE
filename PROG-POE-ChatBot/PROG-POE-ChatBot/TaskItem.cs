using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROG_POE_ChatBot
{
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }

        //dynamically adjusts the reminder according to time/ checks if user wants a reminder
        public string Display
        {
            get
            {
                string baseInfo = Title;

                if (!ReminderDate.HasValue)
                    return baseInfo;

                string status = IsCompleted ? "Completed" : "Pending";
                return $"{baseInfo} - Reminder: {ReminderDate.Value:f} ({status})";
            }
        }

    }

}
