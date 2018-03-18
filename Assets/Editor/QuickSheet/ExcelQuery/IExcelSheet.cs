using System.Collections;

public interface IExcelSheet<DataT>
{
	DataT[] DataArray { get; set; }
}

