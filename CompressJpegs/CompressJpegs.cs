using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom.CodeModule.CompressJpegs
{
    using Emc.InputAccel.CaptureClient;
    using Emc.InputAccel.UimScript;
    using System.IO;
    using PixTools;
    using PixTools.PixImaging;
    using System.Drawing;
    using System.Windows.Forms;
    using global::CompressJpegs;

    public class CompressJpegs : CustomCodeModule
    {
        private ICodeModuleStartInfo _startInfo = null;
        public void CompressNode(IBatchNode node, string TempPath, double Compression)
        {
            //Get the image
            Stream InputImage = new MemoryStream();
            InputImage = node.NodeData.ValueSet.ReadFile("InputImage").ReadData();
            Bitmap fromStream = new Bitmap(InputImage);
            //Convert this to a PixImage
            PixImage Image = new PixImage();
            Image.Import(fromStream);
            //Now we have a bitmap we need to save it to the temporary file location
            //Get the file path
            string fPath = TempPath;
            //Get a node ID
            fPath = fPath + @"\" +  node.NodeData.BatchId + "_" + node.NodeData.NodeId.ToString() + ".jpg";
            //Convert the compression to an int
            int comp = Convert.ToInt32(Compression);
            PixImageStorage.Save(Image, PixFileType.Jpeg, fPath, new PixJpegCompressionSettings(ColorFormat.Rgb, PixCompression.ProgressiveJpeg, comp), OpenFileMode.CreateAlways);
            //Now we want to try and read that file and save it back
            try
            {
                    byte[] f = File.ReadAllBytes(fPath);
                    node.NodeData.ValueSet.WriteFileData("OutputFile", f, ".jpg");
                    //Now delete the files
                    File.Delete(fPath);
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public override bool SetupCodeModule(Control parentWindow, IValueAccessor stepConfiguration)
        {
            //Setup the code module
            using (SetupForm s = new SetupForm())
            {
                //First read the existing values
                s.tempPath = stepConfiguration.ReadString("TempPath");
                s.Compression = stepConfiguration.ReadDouble("Compression");
                if (s.ShowDialog() == DialogResult.OK)
                {
                    stepConfiguration.WriteString("TempPath", s.tempPath);
                    stepConfiguration.WriteDouble("Compression", s.Compression);
                    return true;
                }
            }
            
            // No changes were made.
            return false;
        }
        public override void ExecuteTask(IClientTask task, IBatchContext batchContext)
        {
            try
            {
                //---------------------------------------------------------------------------------------------------------
                // Output some initial trace information about this task.
                //---------------------------------------------------------------------------------------------------------
                _startInfo.Trace("Jpeg Compression - ExecuteTask called.");
                _startInfo.Trace("Jpeg Compression - Date={0} -------------Step={1}, Batch={2}, Node={3}------------",
                        DateTime.Now.ToString(),
                        task.BatchNode.StepData.StepName,
                        task.BatchNode.NodeData.BatchName,
                        task.BatchNode.NodeData.NodeId);
                //Read the configuration values
                string TempPath = task.BatchNode.StepData.StepConfiguration.ReadString("TempPath");
                double Compression = task.BatchNode.StepData.StepConfiguration.ReadDouble("Compression");
                _startInfo.Trace("Using Temp Storage " + TempPath);
                _startInfo.Trace("Compression Set to " + Compression.ToString());
                //First try to see at what level the module is running at
                int RootLevel = task.BatchNode.RootLevel;
                if (RootLevel == 0 )
                {
                    CompressNode(task.BatchNode,TempPath,Compression);
                    task.CompleteTask();
                }
                else
                {
                    //Loop through all of the nodes
                    foreach (IBatchNode node in task.BatchNode.GetDescendantNodes(0))
                    {
                        CompressNode(node,TempPath,Compression);
                    }
                    task.CompleteTask();
                }
            }
            catch(Exception e)
            {
                //---------------------------------------------------------------------------------------------------------
                // Trace the exception and then fail the task.
                //---------------------------------------------------------------------------------------------------------
                _startInfo.Trace("Jpeg Compression - An exception occured. {0}", e.ToString());
                if (task != null)
                {
                    task.FailTask(FailTaskReasonCode.GenericUnrecoverableError, e);
                }
            }
        }
        public override void StopModule()
        {
            _startInfo.Trace("Jpeg Compression Module - StopModule called.");
        }

        public override void StartModule(ICodeModuleStartInfo startInfo)
        {
            _startInfo = startInfo;

            //---------------------------------------------------------------------------------------------------------
            // You can see trace information when you pass the the trace flag on the command-line to the .NET Code Module. 
            // For example: CodeClient.exe -trace:c:\temp\mylog.txt.
            // When the -trace flag is not used, then the Trace() call does nothing.
            //
            // You can use the sc.exe utility with the, config binPath= , option to modify an existing Windows service
            // command-line.
            //---------------------------------------------------------------------------------------------------------
            _startInfo.Trace("Jpeg Compression Module Started - StartModule called.");
           
        }
    }
}
