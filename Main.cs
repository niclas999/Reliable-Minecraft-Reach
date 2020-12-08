using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using Memory;

namespace Reach
{
    public partial class Main : Form
    {
        Mem M = new Mem();

        //Arrays
        string arrayReach = "00 00 00 00 00 00 08 40";
        string arrayReachBuffer = "00 00 00 00 00 00 12 40";

        //Bools
        bool isScanning = false;

        //Enums and Lists
        IEnumerable<long> aReach; 
        IEnumerable<long> aReachBuffer; 
        List<long> lReach = new List<long>(); 
        List<long> lReachBuffer = new List<long>(); 

        //Doubles
        double reachVal;
        double reachBufferVal;

        //Constructor
        public Main()
        {
            InitializeComponent();
        }

        //Form Load
        private async void Main_Load(object sender, EventArgs e)
        {
            M.OpenProcess("javaw"); //Opening the Minecraft process
            await Task.Run(() => scanReach()); //Starting the Reach scan
            tmrReach.Start(); //Starting the Reach process
        }

        //Scanning for 3.0 Double and 4.5 Double
        public async void scanReach()
        {
            isScanning = true;
            aReach = await M.AoBScan(0, 90000000, arrayReach, false, true); //Reading the addresses of 3.0 Double
            aReachBuffer = await M.AoBScan(0, 90000000, arrayReachBuffer, false, true); //Reading the addresses of 4.5 Double
            lReach.AddRange(aReach.ToList()); //Adding all the addresses to a list, so we don't overwrite them when we rescan our reach
            lReachBuffer.AddRange(aReachBuffer.ToList()); // ::
            isScanning = false;
        }

        //Actual Reach process
        private async void tmrReach_Tick(object sender, EventArgs e)
        {
           
            if (M.OpenProcess("javaw") && !isScanning) //Checking if Minecraft is opened and if we are scanning
            {
               
                foreach (var aBuffer in lReachBuffer) //Looping through every address in the List for 4.5 Double
                {
                  
                    foreach (var aReach in lReach) //Looping through every address in the List for 3.0 Double
                    {

                        if (aBuffer.ToString("X").Length == 7 && aReach.ToString("X").Length == 7) //Checking if the 4.5 Double address has a zero in the beginning
                        {

                            if (aBuffer.ToString("X").Substring(0, 3) == aReach.ToString("X").Substring(0, 3)) //Checking if the addresses are similar
                            {
                                reachVal = M.ReadDouble(aReach.ToString("X")); //Scanning the value of 3.0 Double
                                reachBufferVal = M.ReadDouble(aBuffer.ToString("X")); //Scanning the value of 4.5 Double

                                aList.Items.Add("Reach: " + aReach.ToString("X") + " | " + reachVal.ToString()); //Printing the address and value of the address in the Listbox (3.0 Double)                            
                                aList.Items.Add("ReachBuffer: " + aBuffer.ToString("X") + " | " + reachBufferVal.ToString()); //Printing the address and value of the address in the Listbox (4.5 Double)

                                if (reachVal.ToString().Length < 5 && reachVal.ToString().Length > 0)     //Checking if the Length of the 3.0 Double value is min. 1 and max. 4 
                                {                                                                         //Because if the value corrupts and we edit it, it will crash the game.
                                                                                                        
                                    if (chckReach.Checked) //Checking if we want Reach to be enabled
                                    {
                                        await Task.Run(() => M.WriteMemory(aReach.ToString("X"), "double", "6,0"));  //Overwriting 3.0 Double to 6.0
                                        await Task.Run(() => M.WriteMemory(aBuffer.ToString("X"), "double", "7,5")); //Overwriting 4.5 Double to 7.5, because otherwise we'll only have 4.5 blocks of Reach
                                    }
                                    else
                                    {
                                        await Task.Run(() => M.WriteMemory(aReach.ToString("X"), "double", "3,0")); //Changing to the default values if we don't want Reach to be enabled.
                                        await Task.Run(() => M.WriteMemory(aBuffer.ToString("X"), "double", "4,5"));
                                    }

                                }

                            }
                     
                        }                 
                
                    }
              
                }     
         
            }
       
        }

        //Rescanning all the addresses
        private async void tmrRescan_Tick(object sender, EventArgs e)
        {
            if (M.OpenProcess("javaw"))
            {
                await Task.Run(() => scanReach()); //Starting the scan process of Reach
            }
        }
    }
}
