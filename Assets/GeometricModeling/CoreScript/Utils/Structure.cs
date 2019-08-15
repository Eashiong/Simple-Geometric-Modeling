/*
*      
*
*    功能: 数据存储结构扩展
*
*/
namespace System.Collections.Generic
{
    /// <summary>
    /// 双向链表节点
    /// </summary>
    public class LinkNode<T>
    {
        public T data { set; get; }
        public LinkNode<T> next { set; get; }
        public LinkNode<T> last { set; get; }
        public LinkNode(T val, LinkNode<T> last, LinkNode<T> next)
        {
            this.data = val;
            this.last = last;
            this.next = next;
        }
        public LinkNode(LinkNode<T> t)
        {
            data = t.data;
            last = t.last;
            next = t.next;
        }
        

    }

    /// <summary>
    /// 双向循环链表
    /// </summary>
    public class DoubleLink<T>
    {
        //表头
        private readonly LinkNode<T> _linkHead;

        //节点个数
        private int _size;

        //通过索引查找节点
        private LinkNode<T> GetNode(int index)
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException("索引溢出或者链表为空");
            if (index < _size / 2)//正向查找
            {
                LinkNode<T> node = _linkHead.next;
                for (int i = 0; i < index; i++)
                    node = node.next;
                return node;
            }
            //反向查找
            LinkNode<T> rnode = _linkHead.last;
            int rindex = _size - index - 1;
            for (int i = 0; i < rindex; i++)
                rnode = rnode.last;
            return rnode;
        }

        /// <summary>
        /// 构造一个空的双向循环链表
        /// </summary>
        public DoubleLink()
        {
            _linkHead = new LinkNode<T>(default(T), null, null);//双向链表 表头为空
            _linkHead.last = _linkHead;
            _linkHead.next = _linkHead;
            _size = 0;
        }

        /// <summary>
        /// 链表长度
        /// </summary>
        public int GetSize()
        {
            return _size;
        }

        /// <summary>
        /// 非空判断
        /// </summary>
        public bool IsEmpty()
        {
            return _size == 0;
        }

        /// <summary>
        /// 防止index越界
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int NormalIndex(int index)
        {
            if (index > _size - 1)
            {
                index = index - _size;
            }
            else if (index < 0)
            {
                index = _size + index;
            }
            else
                return index;
            return NormalIndex(index);
        }

        /// <summary>
        /// 返回index位置节点
        /// </summary>
        public T Get(int index)
        {
            return GetNode(index).data;
        }
        /// <summary>
        /// 允许索引越界的方式访问节点
        /// </summary>
        public T GetSafe(int index)
        {
            
            if(index > _size-1)
            {
                index = index - _size;
            }
            else if(index < 0 )
            {
                index = _size + index;
            }
            else
                return GetNode(index).data;
            return GetSafe(index);
        }
        /// <summary>
        /// 返回index下一个节点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Next(int index)
        {
            if(index == _size-1)
            {
                index = 0;
            }
            return GetNode(index).data;
        }

        /// <summary>
        /// 返回index上一个节点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Last(int index)
        {
            if (index == 0)
            {
                index = _size - 1;
            }
            return GetNode(index).data;
        }
        /// <summary>
        /// 返回第一个节点
        /// </summary>
        public T GetFirst()
        {
            return GetNode(0).data;
        }

        /// <summary>
        /// 返回最后一个节点
        /// </summary>
        public T GetLast()
        {
            return GetNode(_size - 1).data;
        }

        /// <summary>
        /// 将t插入到第index位置之前
        /// </summary>
        public void Insert(int index, T t)
        {
            if (_size < 1 || index >= _size)
                throw new Exception("没有可插入的点或者索引溢出了");
            if (index == 0)
                Append(_size, t);
            else
            {
                LinkNode<T> inode = GetNode(index);
                LinkNode<T> tnode = new LinkNode<T>(t, inode.last, inode);
                inode.last.next = tnode;
                inode.last = tnode;
                _size++;
            }
        }

        /// <summary>
        /// 追加t到index位置之后
        /// </summary>
        public void Append(int index, T t)
        {
            LinkNode<T> inode;
            
            if (index == 0&&_size == 0)
                inode = _linkHead;
            else
            {
               // index = index - 1;
                if (index < 0)
                    throw new IndexOutOfRangeException("位置不存在");
                inode = GetNode(index);
            }
            LinkNode<T> tnode = new LinkNode<T>(t, inode, inode.next);
            inode.next.last = tnode;
            inode.next = tnode;
            _size++;
        }

        public void AppendLast(T t)
        {           
            Append(Math.Max(_size-1,0), t);
        }

        /// <summary>
        /// 删除第index节点
        /// </summary>
        public void Delete(int index)
        {
            LinkNode<T> inode = GetNode(index);
            inode.last.next = inode.next;
            inode.next.last = inode.last;
            _size--;
        }

        /// <summary>
        /// 删除第一个节点
        /// </summary>
        public void DeleteFirst()
        {
            Delete(0);
        }

        /// <summary>
        /// 删除最后一个节点
        /// </summary>
        public void DeleteLast()
        {
            Delete(_size - 1);
        }
        /// <summary>
        /// 转换为List
        /// </summary>
        /// <returns></returns>
        public List<T> Convert2List()
        {
            List<T> ts = new List<T>();
            for (int i = 0; i < _size; i++)
                ts.Add(Get(i));
            return ts;
        }
        /// <summary>
        /// 打印所有节点
        /// </summary>
        public override string ToString()
        {
            string s = "******************* 链表数据如下 *******************\n";
            for (int i = 0; i < _size; i++)
                s += "items[" + i + "]=" + Get(i).ToString() + ",";
            s += ("\n******************* 链表数据展示完毕 *******************");
            return s;
        }
       
       
}


    /// <summary>
    /// 二叉链表结点类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNode<T>
    {
        public T data;
        public TreeNode<T> left;
        public TreeNode<T> right;

        public TreeNode(T data, TreeNode<T> left, TreeNode<T> right)
        {
            this.data = data;
            this.left = left;
            this.right = right;
        }

        public TreeNode(TreeNode<T> left, TreeNode<T> right)
        {
            this.data = default(T);
            this.left = left;
            this.right = right;
        }
        public TreeNode()
        {

        }
    }

    /// <summary>
    /// 二叉树
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryTree<T>
    {
        private TreeNode<T> head;       //头引用
     

          

        public BinaryTree(T val, TreeNode<T> left, TreeNode<T> right)
        {
            TreeNode<T> p = new TreeNode<T>(val, left, right);
            head = p;
        }

        //判断是否是空二叉树
        public bool IsEmpty()
        {
            if (head == null)
                return true;
            else
                return false;
        }

        //获取根结点
        public TreeNode<T> Root()
        {
            return head;
        }

        //获取结点的左孩子结点
        public TreeNode<T> GetLeft(TreeNode<T> p)
        {
            return p.left;
        }

        public TreeNode<T> GetRight(TreeNode<T> p)
        {
            return p.right;
        }

        //将结点p的左子树插入值为val的新结点，原来的左子树称为新结点的左子树
        public void InsertLeft(T val, TreeNode<T> p)
        {
            TreeNode<T> tmp = new TreeNode<T>(val, null,null);
            tmp.left = p.left;
            p.left = tmp;
        }

        //将结点p的右子树插入值为val的新结点，原来的右子树称为新节点的右子树
        public void InsertRight(T val, TreeNode<T> p)
        {
            TreeNode<T> tmp = new TreeNode<T>(val,null,null);
            tmp.right = p.right;
            p.right = tmp;
        }

        //若p非空 删除p的左子树
        public TreeNode<T> DeleteL(TreeNode<T> p)
        {
            if ((p == null) || (p.left == null))
                return null;
            TreeNode<T> tmp = p.left;
            p.left = null;
            return tmp;
        }

        //若p非空 删除p的右子树
        public TreeNode<T> DeleteR(TreeNode<T> p)
        {
            if ((p == null) || (p.right == null))
                return null;
            TreeNode<T> tmp = p.right;
            p.right = null;
            return tmp;
        }

        //编写算法 在二叉树中查找值为value的结点

        public TreeNode<T> Search(TreeNode<T> root, T value)
        {
            
            TreeNode<T> p = root;
            if (p == null)
                return null;
            if (!p.data.Equals(value))
                return p;
            if (p.left != null)
            {
                return Search(p.left, value);
            }
            if (p.right != null)
            {
                return Search(p.right, value);
            }
            return null;
        }

        //判断是否是叶子结点
        public bool IsLeaf(TreeNode<T> p)
        {
            if ((p != null) && (p.right == null) && (p.left == null))
                return true;
            else
                return false;
        }


        //中序遍历
        //遍历根结点的左子树->根结点->遍历根结点的右子树 
        public void inorder(TreeNode<T> ptr)
        {
            if (IsEmpty())
            {
                throw new Exception("Tree is Empty !");
            }
            if (ptr != null)
            {
                inorder(ptr.left);
                //Console.WriteLine(ptr.Data + " ");
                inorder(ptr.right);
            }
        }


        //先序遍历
        //根结点->遍历根结点的左子树->遍历根结点的右子树 
        public void preorder(TreeNode<T> ptr)
        {
            if (IsEmpty())
            {
                throw new Exception("Tree is Empty !");
            }
            if (ptr != null)
            {
               // Console.WriteLine(ptr.Data + " ");
                preorder(ptr.left);
                preorder(ptr.right);
            }
        }


        //后序遍历
        //遍历根结点的左子树->遍历根结点的右子树->根结点
        public void postorder(TreeNode<T> ptr)
        {
            if (IsEmpty())
            {
                throw new Exception("Tree is Empty !");
            }
            if (ptr != null)
            {
                postorder(ptr.left);
                postorder(ptr.right);
               // Console.WriteLine(ptr.Data + "");
            }
        }


       
    }


}


