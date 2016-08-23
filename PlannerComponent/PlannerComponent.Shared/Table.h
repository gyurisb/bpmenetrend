#ifndef TABLE_H
#define TABLE_H

namespace Storage
{
	template <class T>
	class Table
	{
		char* data;
	public:
		Table() {}
		Table(unsigned char* data) : data((char*)data){}
		Table(char* data) : data(data){}
		Table(T* data) : data((char*)data){}
		Table operator+(int records) const
		{
			return data + records*T::SIZE;
		}
		int operator-(const Table& other) const
		{
			return (data - other.data)/T::SIZE;
		}
		Table operator-(int records) const
		{
			return data - records*T::SIZE;
		}
		T& operator[](int n) { return *((T*)(data + n*T::SIZE)); }
		operator T*() { return (T*)data; }
	};
}

#endif