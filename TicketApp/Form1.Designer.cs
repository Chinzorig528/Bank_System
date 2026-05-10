namespace BankTicket
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTicketNumber = new System.Windows.Forms.Label();
            this.btnGetTicket = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTicketNumber
            // 
            this.lblTicketNumber.AutoSize = true;
            this.lblTicketNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTicketNumber.Location = new System.Drawing.Point(143, 101);
            this.lblTicketNumber.Name = "lblTicketNumber";
            this.lblTicketNumber.Size = new System.Drawing.Size(169, 69);
            this.lblTicketNumber.TabIndex = 0;
            this.lblTicketNumber.Text = "A000";
            this.lblTicketNumber.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnGetTicket
            // 
            this.btnGetTicket.Location = new System.Drawing.Point(145, 197);
            this.btnGetTicket.Name = "btnGetTicket";
            this.btnGetTicket.Size = new System.Drawing.Size(142, 23);
            this.btnGetTicket.TabIndex = 1;
            this.btnGetTicket.Text = "Дугаар авах";
            this.btnGetTicket.UseVisualStyleBackColor = true;
            this.btnGetTicket.Click += new System.EventHandler(this.btnGetTicket_Click_1);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(145, 236);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(142, 23);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Хэвлэх";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnGetTicket);
            this.Controls.Add(this.lblTicketNumber);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTicketNumber;
        private System.Windows.Forms.Button btnGetTicket;
        private System.Windows.Forms.Button btnPrint;
    }
}

