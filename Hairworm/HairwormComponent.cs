﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;


using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Hairworm
{

    public class HairwormComponent : GH_Component, IGH_VariableParameterComponent
    {

		public int clusterParamNumInput = 0;
		public int clusterParamNumOutput = 0;
		private int fixedParamNumInput = 1;
		private int fixedParamNumOutput = 1;
  //      HairwormComponent self = new HairwormComponent();

		string clusterFileUrl = null;
		string fullTempFilePath = null;
		string debugText = "";
        GH_ObjectWrapper[] clusterInputs = null;

        GH_Cluster wormCluster = null;
        GH_Document wormDoc = null;

		#region Methods of GH_Component interface
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public HairwormComponent()
            : base("Hairworm", "Hairworm",
                "Description",
                "Extra", "Hairworm")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("String", "ClusterURL", "URL To Cluster", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Download", "Download", "Download clsuter", GH_ParamAccess.item, false);
//            pManager.AddNumberParameter("Input Value", "InputVal", "InputValue", GH_ParamAccess.item);
//            pManager[0].Optional = true;
            //            pManager.AddGeometryParameter("Input Geometry", "InputGeo", "InputGeometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
//            pManager.AddGeometryParameter("Output Geometry", "OutputGe32o", "OutputGeometry", GH_ParamAccess.tree);
//            pManager.AddGenericParameter("Generic Output", "GenericOutput", "GenericOutput", GH_ParamAccess.tree);
            pManager.AddTextParameter("Debug", "Debug", "This is debug output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

			////////////////////////
            //  Retrieve crucial (fixed) input data, exit if non-existent
			////////////////////////
            if (!DA.GetData(0, ref clusterFileUrl)) { return; }
//            if (!DA.GetData(1, ref downloadCluster)) { return; }

           //GH_Param temptype = new IGH_Param();

			////////////////////////
            // check if cluster was properly loaded, and if parameters are correct
			// and if not, do something about it!
			////////////////////////
            if (wormCluster == null ||
                Params.Input.Count != (fixedParamNumInput + clusterParamNumInput) ||
                Params.Output.Count != (fixedParamNumOutput + clusterParamNumOutput))
            {
                //we've got a parameter mismatch
                // urge user to click on buttom to match paramcount to cluster param count
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cluster not loaded properly - click on 'Reload Cluster' button!");
            }
            else
            {
                //successful! parameters match. so - let's run this thing:

                //get data from hairworm inputs, put into array
                for (int i = fixedParamNumInput; i < (fixedParamNumInput + clusterParamNumInput); i++)
                {
                    if (!DA.GetData(i, ref clusterInputs[i - fixedParamNumInput])) { return; }
                }
                // get data from array, put into cluster
                for (int i = fixedParamNumInput; i < (fixedParamNumInput + clusterParamNumInput); i++)
                {
                    wormCluster.Params.Input[i - fixedParamNumInput].AddVolatileData(new GH_Path(0), 0, clusterInputs[i - fixedParamNumInput]);
                }
                //            wormCluster.Params.Input[0].AddVolatileData(new GH_Path(0), 0, radius);
                //            debugText += "\ninputtypename = " + wormCluster.Params.Input[0].TypeName;

                //get new document, enable it, and add cluster to it
                wormDoc = new GH_Document();
                wormDoc.Enabled = true;
                wormDoc.AddObject(wormCluster, true, 0);

                //            debugText += "\nradisu = " + radius;
                debugText += "\noutputcount = " + wormCluster.Params.Output.Count;
                DA.SetData(0, debugText);

                // Get a pointer to the data inside the first cluster output.
                IGH_Structure data = wormCluster.Params.Output[0].VolatileData;

                // Create a copy of this data (the original data will be wiped)
                DataTree<object> copy = new DataTree<object>();
                copy.MergeStructure(data, new Grasshopper.Kernel.Parameters.Hints.GH_NullHint());

                // Cleanup!
                wormDoc.Enabled = false;
                wormDoc.RemoveObject(wormCluster, false);
                wormDoc.Dispose();
                wormDoc = null;


                // Output
                //            DA.SetDataTree(0, copy); //new Rhino.Geometry.Circle(4.3));
                DA.SetDataTree(1, copy);
            }

            DA.SetData(0, debugText);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{99170264-7e33-48c9-81b9-33d56842aaec}"); }
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new Attributes_Custom(this);
        }

    #endregion

		#region Methods of IGH_VariableParameterComponent interface


        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
			// we want this to be false, because we don't want those pesky users adding their own parameters
			// (but what if a cluster is a variable-input one?)
			// well, we'll deal with that later.
			return false;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
			// see above.
            return false;
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            Param_Number param = new Param_Number();

 /*           param.Name = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
            param.NickName = param.Name;
            param.Description = "Param" + (Params.Input.Count + 1);
            param.SetPersistentData(0.0); */

            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            //Nothing to do here by the moment
            return true;
        }


        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
			//Nothing to do here by the moment
		}

        public void MatchParameterCount()
        {
            while (clusterParamNumOutput != (Params.Output.Count - fixedParamNumOutput))
            {
                if (clusterParamNumOutput > (Params.Output.Count - fixedParamNumOutput))
                    Params.RegisterOutputParam(new Param_GenericObject());
				else
                    Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
            }
            while (clusterParamNumInput != (Params.Input.Count - fixedParamNumInput))
            {
                if (clusterParamNumInput > (Params.Input.Count - fixedParamNumInput))
                    Params.RegisterInputParam(new Param_GenericObject());
				else
                    Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1]);
            }
            clusterInputs = new GH_ObjectWrapper[clusterParamNumInput];
            this.ExpireSolution(true);
        }

        public void ReloadCluster()
        {
            //temporary file url
            //            clusterFileUrl = "https://github.com/provolot/GrasshopperExchange/raw/master/Hairworm/_example_files/SphereMakerVariable.ghcluster";

			////////////////////////
            // set path for temporary file location
			////////////////////////

            string tempPath = System.IO.Path.GetTempPath();
            Uri uri = new Uri(clusterFileUrl);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            fullTempFilePath = tempPath + filename; 


			////////////////////////
            // attempt to downloadCluster file
			////////////////////////
  
			using (WebClient Client = new WebClient())
			{
				try {
					Client.DownloadFile(clusterFileUrl, fullTempFilePath);
                }
				catch(WebException webEx)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Network error: " + webEx.Message);

                }
			}
			debugText += "client.downloadfile( " + clusterFileUrl + ", " + filename + " );\n";
            debugText += tempPath;

            // if gh file doesn't exist in temporary location, abort 
            if (!File.Exists(fullTempFilePath)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File does not exist!"); }

			////////////////////////
            // Create a cluster
			////////////////////////

			// create cluster
            wormCluster = new GH_Cluster();
            wormCluster.CreateFromFilePath(fullTempFilePath);

			// set cluster parameter count
            clusterParamNumInput = wormCluster.Params.Input.Count;
			clusterParamNumOutput = wormCluster.Params.Output.Count;
            debugText += "\ncluster input params # = " + clusterParamNumInput;
            debugText += "\ncluster output params # = " + clusterParamNumOutput;

            MatchParameterCount();
        }


        #endregion
    }
    #region GH_ComponentAttributes interface

    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;
        public Attributes_Custom(GH_Component owner) : base(owner) { thisowner = owner; }

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 22;

            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22;
            rec1.Height = 22;
            rec1.Inflate(-2, -2);

            Bounds = rec0;
            ButtonBounds = rec1;
        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Reload Cluster", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    (base.Owner as HairwormComponent).ReloadCluster();
                    //MessageBox.Show("The button was clicked, and we want " + (base.Owner as HairwormComponent).clusterParamNumInput + " inputs and " + (base.Owner as HairwormComponent).clusterParamNumOutput + " output params", "Button", MessageBoxButtons.OK);

                    return GH_ObjectResponse.Handled;

                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
    #endregion GH_ComponentAttributes interface
}
