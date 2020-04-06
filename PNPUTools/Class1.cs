using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PNPUTools.DataManager
{
	public class PNPU_CLIENT_HABILITATION
	{
		public string USER_ID { get; set; }
		public string CLIENT_ID { get; set; }

		public PNPU_CLIENT_HABILITATION(string USER_ID_, string CLIENT_ID_)
		{
			this.USER_ID = USER_ID_;
			this.CLIENT_ID = CLIENT_ID_;
		}
	}

	public class PNPU_H_REPORT
	{
		public decimal ITERATION { get; set; }
		public decimal WORKFLOW_ID { get; set; }
		public decimal ID_PROCESS { get; set; }
		public string CLIENT_ID { get; set; }
		public string JSON_TEMPLATE { get; set; }

		public PNPU_H_REPORT(decimal ITERATION_, decimal WORKFLOW_ID_, decimal ID_PROCESS_, string CLIENT_ID_, string JSON_TEMPLATE_)
		{
			this.ITERATION = ITERATION_;
			this.WORKFLOW_ID = WORKFLOW_ID_;
			this.ID_PROCESS = ID_PROCESS_;
			this.CLIENT_ID = CLIENT_ID_;
			this.JSON_TEMPLATE = JSON_TEMPLATE_;
		}
	}

	public class PNPU_H_STEP
	{
		public decimal ITERATION { get; set; }
		public decimal WORKFLOW_ID { get; set; }
		public decimal ID_PROCESS { get; set; }
		public string CLIENT_ID { get; set; }
		public string USER_ID { get; set; }
		public DateTime LAUNCHING_DATE { get; set; }
		public DateTime ENDING_DATE { get; set; }
		public string ID_STATUT { get; set; }

		public PNPU_H_STEP(decimal ITERATION_, decimal WORKFLOW_ID_, decimal ID_PROCESS_, string CLIENT_ID_, string USER_ID_, DateTime LAUNCHING_DATE_, DateTime ENDING_DATE_, string ID_STATUT_)
		{
			this.ITERATION = ITERATION_;
			this.WORKFLOW_ID = WORKFLOW_ID_;
			this.ID_PROCESS = ID_PROCESS_;
			this.CLIENT_ID = CLIENT_ID_;
			this.USER_ID = USER_ID_;
			this.LAUNCHING_DATE = LAUNCHING_DATE_;
			this.ENDING_DATE = ENDING_DATE_;
			this.ID_STATUT = ID_STATUT_;
		}
	}

	public class PNPU_H_WORKFLOW
	{
		public decimal ID_H_WORKFLOW { get; set; }
		public string CLIENT_ID { get; set; }
		public decimal WORKFLOW_ID { get; set; }
		public DateTime LAUNCHING_DATE { get; set; }
		public DateTime ENDING_DATE { get; set; }
		public string STATUT_GLOBAL { get; set; }

		public PNPU_H_WORKFLOW()
		{

		}

		public PNPU_H_WORKFLOW(decimal ID_H_WORKFLOW_, string CLIENT_ID_, decimal WORKFLOW_ID_, DateTime LAUNCHING_DATE_, DateTime ENDING_DATE_, string STATUT_GLOBAL_)
		{
			this.ID_H_WORKFLOW = ID_H_WORKFLOW_;
			this.CLIENT_ID = CLIENT_ID_;
			this.WORKFLOW_ID = WORKFLOW_ID_;
			this.LAUNCHING_DATE = LAUNCHING_DATE_;
			this.ENDING_DATE = ENDING_DATE_;
			this.STATUT_GLOBAL = STATUT_GLOBAL_;
		}
	}


	public class PNPU_LOG
	{
		public decimal ID_LOG { get; set; }
		public decimal ID_PROCESS { get; set; }
		public decimal ITERATION { get; set; }
		public decimal WORKFLOW_ID { get; set; }
		public string MESSAGE { get; set; }
		public string STATUT_MESSAGE { get; set; }
		public string ID_CONTROLE { get; set; }
		public string IS_CONTROLE { get; set; }
		public DateTime DATE_LOG { get; set; }
		public string SERVER { get; set; }
		public string BASE { get; set; }
		public string NIVEAU_LOG { get; set; }

		public PNPU_LOG(decimal ID_LOG_, decimal ID_PROCESS_, decimal ITERATION_, decimal WORKFLOW_ID_, string MESSAGE_, string STATUT_MESSAGE_, string ID_CONTROLE_, string IS_CONTROLE_, DateTime DATE_LOG_, string SERVER_, string BASE_, string NIVEAU_LOG_)
		{
			this.ID_LOG = ID_LOG_;
			this.ID_PROCESS = ID_PROCESS_;
			this.ITERATION = ITERATION_;
			this.WORKFLOW_ID = WORKFLOW_ID_;
			this.MESSAGE = MESSAGE_;
			this.STATUT_MESSAGE = STATUT_MESSAGE_;
			this.ID_CONTROLE = ID_CONTROLE_;
			this.IS_CONTROLE = IS_CONTROLE_;
			this.DATE_LOG = DATE_LOG_;
			this.SERVER = SERVER_;
			this.BASE = BASE_;
			this.NIVEAU_LOG = NIVEAU_LOG_;
		}
	}

	public class PNPU_PARAMETERS
	{
		public string PARAMETER_ID { get; set; }
		public string PARAMETER_VALUE { get; set; }

		public PNPU_PARAMETERS(string PARAMETER_ID_, string PARAMETER_VALUE_)
		{
			this.PARAMETER_ID = PARAMETER_ID_;
			this.PARAMETER_VALUE = PARAMETER_VALUE_;
		}
	}

	public class PNPU_STATUT
	{
		public string ID_STATUT { get; set; }
		public string MESSAGE_STATUT { get; set; }

		public PNPU_STATUT(string ID_STATUT_, string MESSAGE_STATUT_)
		{
			this.ID_STATUT = ID_STATUT_;
			this.MESSAGE_STATUT = MESSAGE_STATUT_;
		}
	}

	public class PNPU_STEP
	{
		public int ID_ORDER { get; set; }
		public string ID_PROCESS { get; set; }
		public string ID_WORKFLOW { get; set; }

		public PNPU_STEP()
		{ }

		public PNPU_STEP(int ID_ORDER_, string ID_PROCESS_, string ID_WORKFLOW_)
		{
			this.ID_ORDER = ID_ORDER_;
			this.ID_PROCESS = ID_PROCESS_;
			this.ID_WORKFLOW = ID_WORKFLOW_;
		}
	}

	public class PNPU_USER
	{
		public string USER_ID { get; set; }
		public string USER_PROFILE { get; set; }

		public PNPU_USER(string USER_ID_, string USER_PROFILE_)
		{
			this.USER_ID = USER_ID_;
			this.USER_PROFILE = USER_PROFILE_;
		}
	}

	public class PNPU_WORKFLOW
	{
		public decimal WORKFLOW_ID { get; set; }
		public string WORKFLOW_LABEL { get; set; }

		public PNPU_WORKFLOW() { }
		public PNPU_WORKFLOW(decimal WORKFLOW_ID_, string WORKFLOW_LABEL_)
		{
			this.WORKFLOW_ID = WORKFLOW_ID_;
			this.WORKFLOW_LABEL = WORKFLOW_LABEL_;
		}
	}

	public class PNPU_WORKFLOW_HABILITATION
	{
		public decimal WORKFLOW_ID { get; set; }
		public string USER_PROFILE { get; set; }

		public PNPU_WORKFLOW_HABILITATION(decimal WORKFLOW_ID_, string USER_PROFILE_)
		{
			this.WORKFLOW_ID = WORKFLOW_ID_;
			this.USER_PROFILE = USER_PROFILE_;
		}
	}


	public class InfoClientStep
	{
		public string CLIENT_ID { get; set; }
		public DateTime LAUNCHING_DATE { get; set; }
		public string ID_STATUT { get; set; }
		public string PROCESS_LABEL { get; set; }
		public int ORDER_ID { get; set; }
		public decimal PERCENTAGE_COMPLETUDE { get; set; }
		public string TYPOLOGY { get; set; }

		public InfoClientStep() { }
	}

	public class PNPU_PROCESS
	{
		public decimal ID_PROCESS { get; set; }
		public string PROCESS_LABEL { get; set; }
		public string IS_LOOPABLE { get; set; }

		public PNPU_PROCESS()
		{
		}
		public PNPU_PROCESS(decimal ID_PROCESS_, string PROCESS_LABEL_, string IS_LOOPABLE_)
		{
			this.ID_PROCESS = ID_PROCESS_;
			this.PROCESS_LABEL = PROCESS_LABEL_;
			this.IS_LOOPABLE = IS_LOOPABLE_;
		}
	}

	public static class Helper
	{
		private static readonly IDictionary<Type, ICollection<PropertyInfo>> _Properties =
			new Dictionary<Type, ICollection<PropertyInfo>>();

		/// <summary>
		/// Converts a DataTable to a list with generic objects
		/// </summary>
		/// <typeparam name="T">Generic object</typeparam>
		/// <param name="table">DataTable</param>
		/// <returns>List with generic objects</returns>
		public static IEnumerable<T> DataTableToList<T>(this DataTable table) where T : class, new()
		{
			try
			{
				var objType = typeof(T);
				ICollection<PropertyInfo> properties;

				lock (_Properties)
				{
					if (!_Properties.TryGetValue(objType, out properties))
					{
						properties = objType.GetProperties().Where(property => property.CanWrite).ToList();
						_Properties.Add(objType, properties);
					}
				}

				var list = new List<T>(table.Rows.Count);

				//foreach (var row in table.AsEnumerable().Skip(1))
				foreach (var row in table.AsEnumerable())
				{
					var obj = new T();

					foreach (var prop in properties)
					{
						try
						{
							var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
							var safeValue = row[prop.Name] == null ? null : Convert.ChangeType(row[prop.Name], propType);

							prop.SetValue(obj, safeValue, null);
						}
						catch(Exception exc)
						{
							// ignored
						}
					}

					list.Add(obj);
				}

				return list;
			}
			catch
			{
				return Enumerable.Empty<T>();
			}
		}
	}
}
