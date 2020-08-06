using System;
using System.Collections.Generic;

namespace PNPUTools
{

    public class AnalyseResultFile
    {
        public String pathFile { get; set; }
        public String fileName { get; set; }

        readonly List<AnalyseResultLine> listLine;

        public List<AnalyseResultLine> ListLine()
        {
            return listLine;
        }

        public AnalyseResultFile(string pathFile_, string fileName_)
        {
            pathFile = pathFile_;
            fileName = fileName_;
            listLine = new List<AnalyseResultLine>();
        }

        public void addLine(AnalyseResultLine resultLine)
        {
            listLine.Add(resultLine);
        }
    }

    public class AnalyseResultLine
    {
        public String Result { get; set; }
        public String AnalysedFlag { get; set; }
        public String PersistFlag { get; set; }
        public String TransferFlag { get; set; }
        public String OriginDestination { get; set; }
        public String CommandDetail { get; set; }
        public String Sequence { get; set; }
        public String Package { get; set; }
        public String ObjectType { get; set; }
        public String IdObject { get; set; }
        public String IdObject2 { get; set; }
        public String IdObject3 { get; set; }
        public String IdObject4 { get; set; }
        public String Comments { get; set; }

        public AnalyseResultLine(string result, string analysedFlag, string persistFlag, string transferFlag, string originDestination, string commandDetail, string sequence, string package, string objectType, string idObject, string idObject2, string idObject3, string idObject4, string comments)
        {
            Result = result;
            AnalysedFlag = analysedFlag;
            PersistFlag = persistFlag;
            TransferFlag = transferFlag;
            OriginDestination = originDestination;
            CommandDetail = commandDetail;
            Sequence = sequence;
            Package = package;
            ObjectType = objectType;
            IdObject = idObject;
            IdObject2 = idObject2;
            IdObject3 = idObject3;
            IdObject4 = idObject4;
            Comments = comments;
        }

        public AnalyseResultLine(string[] line)
        {
            Result = line[0];
            AnalysedFlag = line[1];
            PersistFlag = line[2];
            TransferFlag = line[3];
            OriginDestination = line[4];
            CommandDetail = line[5];
            Sequence = line[6];
            Package = line[7];
            ObjectType = line[8];
            IdObject = line[9];
            IdObject2 = line[10];
            IdObject3 = line[11];
            IdObject4 = line[12];
            Comments = line[13];
        }
    }
}
