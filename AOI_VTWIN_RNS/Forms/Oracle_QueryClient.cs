using System;
using System.Data;
using System.Windows.Forms;

using CollectorPackage.Src.Database;

namespace CollectorPackage
{
    public partial class Oracle_QueryClient : Form
    {
        public OracleConnector oracle = null;
        public string RemoteQuery ="";

        public Oracle_QueryClient()
        {
            InitializeComponent();
        }

        private void TestOracle_Load(object sender, EventArgs e)
        {
            if (!RemoteQuery.Equals(""))
            {
                textBox1.Text = RemoteQuery;
                OracleQuery(RemoteQuery);
            }
        }

        private void OracleQuery(string query)
        {
            try
            {
                DataTable dt = oracle.Query(query);
                gridOracle.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ejecutarQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string query = textBox1.Text;
            if (!query.Trim().Equals(""))
            {
                OracleQuery(query);
            }
        }
        
        private void gridOracle_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            if (row >= 0 && col >= 0)
            {
                DataGridViewRow drow = gridOracle.Rows[row];
                string columnName = gridOracle.Columns[col].Name;

                switch (columnName)
                {
                    case "PCB_NAME":
                        Oracle_QueryClient newWin = new Oracle_QueryClient();
                        newWin.RemoteQuery =  QueryDetalle(
                                                drow.Cells["PCB_NO"].Value.ToString(),
                                                drow.Cells["TEST_MACHINE_ID"].Value.ToString(),
                                                drow.Cells["SAVED_MACHINE_ID"].Value.ToString(),
                                                drow.Cells["PROGRAM_NAME_ID"].Value.ToString(),
                                                drow.Cells["REVISION_NO"].Value.ToString(),
                                                drow.Cells["SERIAL_NO"].Value.ToString(),
                                                drow.Cells["LOAD_COUNT"].Value.ToString()
                                            );
                        newWin.Show();
                    break;
                }

            }
        }

        private string QueryDetalle(string pcb_no, string test_machine_id, string saved_machine_id, string program_name_id, string revision_no, string serial_no, string load_count) {
            string query = @"
SELECT 

	CI.COMPONENT_NAME,
	CI.COMPONENT_BLOCK_NO,
	FCI.PCB_NO,

    (CASE WHEN FCI.REVISED_FAULT_ID IS NULL 
        THEN (CASE WHEN ( FCI.COMPONENT_PERSON_REVISOR IS NULL OR FCI.COMPONENT_REVISE_END_DATE IS NULL )
                    THEN 'SIN_VERIFICAR' 
                    ELSE 'FALSO' 
              END) 
        ELSE 'REAL' 
    END) AS ERROR,

    FCI.COMPONENT_REVISE_END_DATE,
	FCI.COMPONENT_PERSON_REVISOR, 


    FCI.FAULT_CODE,
	FCI.MACHINE_FAULT_CODE,
	FCI.REVISED_FAULT_ID

FROM 

	FAULT_COMPONENT_INFO FCI,
	COMPONENT_INFO CI

WHERE 

	CI.SAVED_MACHINE_ID = FCI.SAVED_MACHINE_ID AND
	CI.PROGRAM_NAME_ID = FCI.PROGRAM_NAME_ID AND
	CI.REVISION_NO = FCI.REVISION_NO  AND
	CI.SERIAL_NO = FCI.SERIAL_NO  AND
	CI.COMPONENT_NO = FCI.COMPONENT_NO AND

	FCI.PCB_NO =  " + pcb_no+@" AND
	FCI.TEST_MACHINE_ID = " + test_machine_id + @" AND
	FCI.SAVED_MACHINE_ID =  " + saved_machine_id + @" AND
	FCI.PROGRAM_NAME_ID =  " + program_name_id + @" AND
	FCI.REVISION_NO =  " + revision_no + @" AND
	FCI.SERIAL_NO =  " + serial_no + @" AND
	FCI.LOAD_COUNT =  " + load_count + @" 

ORDER BY 

    ERROR DESC,
	FCI.COMPONENT_PERSON_REVISOR ASC
";

            return query;
        }

        // Quick Querys
        private void inspeccionesDeLineaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"
SELECT 

	PNI.PCB_NAME,

    TO_CHAR(PCI.END_DATE, 'YYYY-MM-DD') AS AOI_FECHA, 
    TO_CHAR(PCI.END_DATE, 'HH24:MI:SS') AS AOI_HORA,  	

    TO_CHAR(PCI.REVISE_END_DATE, 'YYYY-MM-DD') AS INSP_FECHA, 
    TO_CHAR(PCI.REVISE_END_DATE, 'HH24:MI:SS') AS INSP_HORA,  	

    PCI.END_DATE,
    PCI.REVISE_END_DATE,

	PCI.TEST_RESULT,
	PCI.REVISE_RESULT,
	PCI.BARCODE,
	PCI.MACHINE_DETERMINATION,

	PCI.PCB_NO,
    PCI.TEST_MACHINE_ID,
    PCI.SAVED_MACHINE_ID,
    PCI.PROGRAM_NAME_ID,
    PCI.REVISION_NO,
    PCI.SERIAL_NO,
    PCI.LOAD_COUNT


FROM 
	PROGRAM_INFO PRI,
	PCB_INFO PCI,
	PCB_NAME_INFO PNI  

WHERE 	
    PRI.PCB_ID = PNI.PCB_ID  AND 
	
--  (PCI.REVISE_END_DATE IS NOT NULL OR PCI.MACHINE_DETERMINATION IS NULL) AND 
--  (PCI.REVISE_RESULT IS NOT NULL) AND 

	PRI.PROGRAM_NAME_ID = PCI.PROGRAM_NAME_ID  AND 
	PRI.SAVED_MACHINE_ID = PCI.SAVED_MACHINE_ID  AND 
	PRI.PROGRAM_NAME_ID = PCI.PROGRAM_NAME_ID  AND 
	PRI.REVISION_NO = PCI.REVISION_NO  AND 
	PRI.SERIAL_NO = PCI.SERIAL_NO   AND 
	
	PRI.CREATE_MACHINE_ID = 4 AND 
	PCI.END_DATE > TO_DATE('2014-05-07 09:45:00', 'YYYY-MM-DD HH24:MI:SS')  

ORDER BY 
	PCI.END_DATE DESC

";
        }
        private void detalleDeInspeccionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"
SELECT 

	CI.COMPONENT_NAME,
	CI.COMPONENT_BLOCK_NO,
	FCI.PCB_NO,
	FCI.COMPONENT_REVISE_END_DATE,
	FCI.COMPONENT_PERSON_REVISOR, 
	FCI.FAULT_CODE,
	FCI.MACHINE_FAULT_CODE,
CASE WHEN FCI.FAULT_CODE = FCI.MACHINE_FAULT_CODE THEN 'FALSO' ELSE 'REAL' END AS ERROR


FROM 

	FAULT_COMPONENT_INFO FCI,
	COMPONENT_INFO CI

WHERE 

	CI.SAVED_MACHINE_ID = FCI.SAVED_MACHINE_ID AND
	CI.PROGRAM_NAME_ID = FCI.PROGRAM_NAME_ID AND
	CI.REVISION_NO = FCI.REVISION_NO  AND
	CI.SERIAL_NO = FCI.SERIAL_NO  AND
	CI.COMPONENT_NO = FCI.COMPONENT_NO AND

	FCI.PCB_NO =  4 AND
	FCI.TEST_MACHINE_ID = 11 AND
	FCI.SAVED_MACHINE_ID =  3 AND
	FCI.PROGRAM_NAME_ID =  282 AND
	FCI.REVISION_NO =  6 AND
	FCI.SERIAL_NO =  12 AND
	FCI.LOAD_COUNT =  1 

ORDER BY 

ERROR DESC,
	FCI.COMPONENT_PERSON_REVISOR ASC
";
        }
        private void detalleAgrupadoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"
SELECT 


    (CASE WHEN FCI.REVISED_FAULT_ID IS NULL 
        THEN (CASE WHEN ( FCI.COMPONENT_PERSON_REVISOR IS NULL OR FCI.COMPONENT_REVISE_END_DATE IS NULL )
                    THEN 'SIN_VERIFICAR' 
                    ELSE 'FALSO' 
              END) 
        ELSE 'REAL' 
    END) AS ERROR,

CI.COMPONENT_NAME,
CI.COMPONENT_BLOCK_NO,
FCI.FAULT_CODE

FROM 

	FAULT_COMPONENT_INFO FCI,
	COMPONENT_INFO CI

WHERE 

	CI.SAVED_MACHINE_ID = FCI.SAVED_MACHINE_ID AND
	CI.PROGRAM_NAME_ID = FCI.PROGRAM_NAME_ID AND
	CI.REVISION_NO = FCI.REVISION_NO  AND
	CI.SERIAL_NO = FCI.SERIAL_NO  AND
	CI.COMPONENT_NO = FCI.COMPONENT_NO AND

	FCI.PCB_NO =  185 AND
	FCI.TEST_MACHINE_ID = 4 AND
	FCI.SAVED_MACHINE_ID =  3 AND
	FCI.PROGRAM_NAME_ID =  265 AND
	FCI.REVISION_NO =  2 AND
	FCI.SERIAL_NO =  111 AND
	FCI.LOAD_COUNT =  3 

GROUP BY

(CASE WHEN FCI.REVISED_FAULT_ID IS NULL 
        THEN (CASE WHEN ( FCI.COMPONENT_PERSON_REVISOR IS NULL OR FCI.COMPONENT_REVISE_END_DATE IS NULL )
                    THEN 'SIN_VERIFICAR' 
                    ELSE 'FALSO' 
              END) 
        ELSE 'REAL' 
    END),

CI.COMPONENT_NAME,
CI.COMPONENT_BLOCK_NO,
FCI.FAULT_CODE

ORDER BY

ERROR DESC";
        }
    }
}
