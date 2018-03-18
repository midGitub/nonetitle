using System.Collections;
using System.Collections.Generic;

public abstract class UserDataBase
{
	public virtual void Read(ES2Reader reader)
	{
	}

	public virtual void Write(ES2Writer writer)
	{
	}

	protected abstract string GetFileName();

	protected UserDataBase()
	{
		Reset();
		Load();
		Save();
	}

	public virtual void Reset()
	{
	}

	public virtual void Save()
	{
		#if !CORE_DLL
		using(ES2Writer writer = ES2Writer.Create(GetFileName()))
		{
			Write(writer);
			writer.Save();
		}
		#endif
	}

	public virtual void Load()
	{
		#if !CORE_DLL
		string name = GetFileName();
		if(!ES2.Exists(name))
		{
			using(ES2Writer writer = ES2Writer.Create(GetFileName()))
			{
				writer.Save();
			}
		}

		using(ES2Reader reader = ES2Reader.Create(GetFileName()))
		{
			Read (reader);
		}
		#endif
	}

	public bool IsTagExist(string fieldName)
	{
		return ES2.Exists(GetFileName() + "?Tag=" + fieldName);
	}

	protected T ReadTag<T>(ES2Reader reader, string tag, T defaultValue)
	{
		T result = defaultValue;
		if(IsTagExist(tag))
			result = reader.Read<T>(tag);
		return result;
	}
}
