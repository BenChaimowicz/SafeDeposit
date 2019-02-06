using System;
using System.IO;
using Newtonsoft.Json;

namespace SafeDeposit
{

    public enum SafeDoor
    { 
        Open,
        Locked,
        ClosedUnlocked
    }
    [JsonObject]
    class Safe
    {
        [JsonProperty]
        private int[] KeyCode { get; set; } = new int[4] { 1, 2, 3, 4 };
        private SafeDoor safeDoor = SafeDoor.ClosedUnlocked;
        [JsonIgnore]
        public int LastKey { get; set; }
        [JsonIgnore]
        readonly string DestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Safe.txt");




        private readonly string[] allKeys = new string[] { "r", "R", "c", "C", "o", "O", "l", "L" };


        public Safe()
        {
            if (File.Exists(DestPath))
            {
                LoadFile();
            } else { SerializeSafe(); }
            ReportStatus();
            DisplayMenu();
        }

        public void KeyPressed(string key)
        {
            string msg = "";
            if (!int.TryParse(key, out int cmd))
            {
                int i = Array.FindIndex(allKeys, ele => ele == key);
                if (i > -1)
                {
                    VerifyCmd(i);
                }
                else
                {
                    msg = "No such action.";
                }
            }
            else
            {
                if (key.Length == 4)
                {
                    if (CheckCode(key))
                    {
                        msg = UnlockDoor();
                    }
                    else
                    {
                        msg = "Incorrect PIN!";
                    }
                } else
                {
                    msg = "Invalid action. PIN must be 4 digits long.";
                }
            }
            Console.Clear();
            ReportStatus();
            DisplayMenu(msg);
        }
        private void DisplayMenu(string msg = "")
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            string menu = "Available actions: \n" +
                "Enter PIN to unlock safe \n" +
                "R = Reset PIN. \n" +
                "C = Close Door \n" +
                "O = Open Door \n" +
                "L = Lock \n" +
                "Please type your command: ";
            Console.WriteLine(menu);
            SetMsgColor();
            Console.WriteLine(msg);
            KeyPressed(Console.ReadLine());
        }

        private void VerifyCmd(int key)
        {
            string msg;
            switch (key)
            {
                case 0:
                case 1:
                    msg = ResetCode();
                    break;
                case 2:
                case 3:
                    msg = CloseDoor();
                    break;
                case 4:
                case 5:
                    msg = OpenDoor();
                    break;
                case 6:
                case 7:
                    msg = LockDoor();
                    break;
                default:
                    msg = "";
                    break;
            }
            Console.Clear();
            ReportStatus();
            DisplayMenu(msg);
        }

        private bool CheckCode(string code)
        {
            bool isCorrect = true;
            for (int i = 0; i < code.Length; i++)
            {
                int intToCheck = Convert.ToInt32(char.GetNumericValue(code[i]));
                if (intToCheck != KeyCode[i])
                {
                    isCorrect = false;
                }
            }
            return isCorrect;
        }

        private string ResetCode()
        {
            SetMsgColor();

            bool validCode = false;
            do
            {
                Console.WriteLine("Enter Old PIN: (4 Digits only)");
                string oldCode = Console.ReadLine();
                if (CheckCode(oldCode))
                {
                    Console.WriteLine("Enter New PIN: (4 Digits only)");
                    string newCode = Console.ReadLine();
                    if (newCode.Length == 4 && int.TryParse(newCode, out int result))
                    {
                        KeyCode = new int[]
                            {Convert.ToInt32(char.GetNumericValue(newCode[0])),
                        Convert.ToInt32(char.GetNumericValue(newCode[1])),
                        Convert.ToInt32(char.GetNumericValue(newCode[2])),
                        Convert.ToInt32(char.GetNumericValue(newCode[3]))};
                        SerializeSafe();
                        return "New PIN Set!";
                    }

                }

            } while (!validCode);
            return "";
        }

        private void SerializeSafe()
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(DestPath, json);
        }

        private void LoadFile()
        {
            JsonConvert.PopulateObject(File.ReadAllText(DestPath), this);
        }

        private string CloseDoor()
        {
            SetMsgColor();

            if (safeDoor != SafeDoor.Open)
            {
                return "Door is already closed.";
            } else
            {
                safeDoor = SafeDoor.ClosedUnlocked;
                return "";
            };
        }

        private string OpenDoor()
        {
            SetMsgColor();

            if (safeDoor == SafeDoor.Open)
            {
                return "Door is already open.";
            } else if (safeDoor == SafeDoor.Locked) {
                return "Door is locked, you need to unlock it.";
            } else
            {
                safeDoor = SafeDoor.Open;
                return "";
            }
        }

        private string LockDoor()
        {
            SetMsgColor();

            if (safeDoor == SafeDoor.Open)
            {
                return "Door must be closed before locking it.";
            }
            else if (safeDoor == SafeDoor.Locked)
            {
                return "Door is already locked.";
            } else
            {
                safeDoor = SafeDoor.Locked;
                return "";
            }
        }

        private string UnlockDoor()
        {
            safeDoor = SafeDoor.ClosedUnlocked;
            return "";
        }

        public void ReportStatus()
        {
            string report =
                "Safe Status: \n" +
                "Door: " + DoorStatus()[0] + "\n" +
                "Lock: " + DoorStatus()[1] + "\n";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(report);
        }
        private string[] DoorStatus()
        {
            SetMsgColor();
            if (safeDoor == SafeDoor.ClosedUnlocked)
            {
                return new string[]{ "Closed", "Unlocked"};
            } else if (safeDoor == SafeDoor.Locked)
            {
                return new string[] { "Closed", "Locked" };
            } else
            {
                return new string[] { "Open", "Unlocked" };
            }
        }

        private void SetMsgColor()
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
