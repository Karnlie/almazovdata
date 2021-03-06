﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphVis
{
	abstract class CalculationSystem
	{
		abstract protected void update(int userCommand);
		abstract protected void initCalculations();
		abstract protected void resetState();

		protected int numIterations;
		public LayoutSystem HostSystem { get; set; }

		public CalculationSystem(LayoutSystem host)
		{
			HostSystem = host;
			numIterations = 0;
		}

		public void Reset()
		{
			resetState();
		}

		public void Initialize()
		{
			initCalculations();
		}

		public void Update(int userCommand)
		{
			update(userCommand);
		}

		public int NumberOfIterations
		{
			get { return numIterations; }
		}

	}
}
