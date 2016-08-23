

#ifndef LINQ_H
#define LINQ_H

namespace std
{
	#pragma pack(push, 1)
	template <class TVal>
	class ArrayPt
	{
		TVal* begin_;
		size_t size_;
	public:
		ArrayPt() {}
		ArrayPt(TVal* begin, size_t size) : begin_(begin), size_(size) {}
		TVal* begin() { return begin_; }
		TVal* end() { return begin_ + size_; }

		int size() { return size_; }
		TVal& operator[](int i) { return begin_[i]; }
	};
	#pragma pack(pop)

	template <class NumberType, class Iter, class Func>
	NumberType sum(Iter begin, Iter end, Func pred)
	{
		NumberType nr = 0;
		for (;begin!=end; ++begin)
			nr += pred(*begin);
		return nr;
	}

	template <class T, class Iter, class Pred>
	T& minBy(Iter begin, Iter end, Pred pred)
	{
		T* min = NULL;
		for (;begin!=end; ++begin)
			if (min == NULL || pred(*begin, *min))
				min = &*begin;
		return *min;
	}
	template <class T, class Container, class Pred>
	T minBy(Container cont, Pred pred)
	{
		return minBy<T>(cont.begin(), cont.end(), pred);
	}

	template <typename T> int sgn(T val) {
		return (T(0) < val) - (val < T(0));
	}
	
	template <class Iter, class Func>
	bool any(Iter begin, Iter end, Func pred)
	{
		for (; begin!=end; ++begin)
			if (pred(*begin))
				return true;
		return false;
	}
	template <class Container, class Func>
	bool any(Container cont, Func pred)
	{
		return any(cont.begin(), cont.end(), pred);
	}
	
	template <class Iter, class Func>
	bool all(Iter begin, Iter end, Func pred)
	{
		for (; begin!=end; ++begin)
			if (!pred(*begin))
				return false;
		return true;
	}
	template <class Container, class Func>
	bool all(Container cont, Func pred)
	{
		return all(cont.begin(), cont.end(), pred);
	}

	template <class TVal, class Iter, class Pred>
	list<TVal> filter(Iter begin, Iter end, Pred pred)
	{
		list<TVal> list;
		for (;begin!=end; ++begin)
			if (pred(*begin))
				list.push_back(*begin);
		return list;
	}
	template <class TVal, class Container, class Pred>
	list<TVal> filter(Container cont, Pred pred)
	{
		return filter<TVal>(cont.begin(), cont.end(), pred);
	}
	
	/*
	template <class TRes, class Iter, class Func>
	list<TRes> map(Iter begin, Iter end, Func selector)
	{
		list<TRes> list;
		for (;begin!=end; ++begin)
			list.push_back(selector(*begin));
		return list;
	}
	template <class TRes, class Container, class Pred>
	list<TRes> map(Container cont, Pred pred)
	{
		return map<TRes>(cont.begin(), cont.end(), pred);
	}
	*/

	template <class T, class Func, class Res>
	int binarySearch(T* arr, int size, Res element, Func selector)
	{
		int begin = 0, end = size;
		while (1)
		{
			int middle = (begin + end) / 2;
			Res res = selector(arr[middle]);
			if (res == element)
			{
				return middle;
			}

			if (begin == end - 1 || begin == end)
			{
				if (res < element)
					return -end - 1;
				else 
					return -begin - 1;
			}

			if (res < element)
				begin = middle + 1;
			else
				end = middle;
		}
	}
	
	template<class T, class Iter1, class Iter2>
	set<T> Union(Iter1 begin1, Iter1 end1, Iter2 begin2, Iter2 end2)
	{
		set<T> set;
		for (; begin1 != end1; ++begin1)
			set.insert(*begin1);
		for (; begin2 != end2; ++begin2)
			set.insert(&*begin2);	//ez itt csalás
		return set;
	}
	template<class T, class Container1, class Container2>
	set<T> Union(Container1 cont1, Container2 cont2)
	{
		return Union<T>(cont1.begin(), cont1.end(), cont2.begin(), cont2.end());
	}

}

#endif