using System;
using System.Collections.Generic;

namespace PNPUTools
{

    public class AnalyseResultFile
    {
        public String pathFile;
        public String fileName;
        List<AnalyseResultLine> listLine;

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
        public String Result;
        public String AnalysedFlag;
        public String PersistFlag;
        public String TransferFlag;
        public String OriginDestination;
        public String CommandDetail;
        public String Sequence;
        public String Package;
        public String ObjectType;
        public String IdObject;
        public String IdObject2;
        public String IdObject3;
        public String IdObject4;
        public String Comments;

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
