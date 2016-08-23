using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace CityTransitApp.WPSilverlight.PageElements.SearchElements
{
    public interface CategoryTreeNode
    {
        IList<CategoryTreeNode> Children { get; }
        string Name { get; }
        Uri Icon { get; }
        IEnumerable<RouteGroup> Values { get; }
        bool HasChildren { get; }
    }
    public class CategoryTreeLeaf : CategoryTreeNode
    {
        public RouteSelector RouteSelector;
        public string Name { get; set; }
        public Uri Icon { get; set; }

        public IList<CategoryTreeNode> Children
        {
            get { return null; }
        }
        public IEnumerable<RouteGroup> Values
        {
            get { return RouteSelector(); }
        }
        public bool HasChildren { get { return false; } }
    }
    public class CategoryTreeInnerNode : CategoryTreeNode
    {
        public Uri Icon { get; set; }
        public IList<CategoryTreeNode> Children { get; set; }
        public string Name { get; set; }


        public IEnumerable<RouteGroup> Values
        {
            get { return null; }
        }
        public bool HasChildren { get { return Children != null && Children.Count > 0; } }
    }

    public class CategoryTreeAdapter
    {
        public IList<CategoryTreeNode> TopCategories { get; private set; }

        public CategoryTreeAdapter(CategoryTree tree)
        {
            var root = createNode("root", tree);
            TopCategories = root.Children;
        }

        private CategoryTreeNode createNode(string key, object node)
        {
            var keys = key.Split(';');
            string name = keys[0];
            Uri icon = null;
            if (keys.Length > 1)
                icon = new Uri("/Assets/Categories/" + keys[1], UriKind.Relative);

            if (node is CategoryTree)
            {
                CategoryTree root = node as CategoryTree;
                return new CategoryTreeInnerNode
                {
                    Name = name,
                    Icon = icon,
                    Children = root.Select(x => createNode(x.Key, x.Value)).ToList()
                };
            }
            else
            {
                return new CategoryTreeLeaf
                {
                    Name = name,
                    Icon = icon,
                    RouteSelector = node as RouteSelector
                };
            }
        }
    }
}
