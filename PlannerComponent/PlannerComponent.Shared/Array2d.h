#ifndef ARRAY2D_H
#define ARRAY2D_H

namespace std
{
	template <class T>
	class Array2d
	{
		T* data;
		int rowLength;
		int rowCount;
	public:
		Array2d() {}
		Array2d(int rows, int cols) : data(new T[rows*cols]()), rowLength(cols), rowCount(rows) {}
		~Array2d() { delete[] data; }
		T* operator[](int row) { return data + row*rowLength; }
		int GetLength(int dimension)
		{
			if (!dimension) return rowCount;
			else return rowLength;
		}
		T* begin() { return data; }
		T* end() { return data + rowCount*rowLength; }
	};
}

#endif