﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using global::sln_SingleApartment.Models;
using Newtonsoft.Json;
using sln_SingleApartment.ViewModels;
using PagedList;
using sln_SingleApartment.ViewModel;
using System.Net.Http;

using HttpMethod = System.Net.Http.HttpMethod;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using static sln_SingleApartment.Models.CUser;

namespace sln_SingleApartment.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        #region index.html
        public ActionResult Home()
        {

            SingleApartmentEntities db = new SingleApartmentEntities();

            //登入
            //============================================
            var user = Session[CDictionary.welcome] as CMember;

            if (user == null) { return RedirectToAction("Login", "Member"); }

            CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };

            //如果活動ID有東西將商品加入到HOME 此為團購商品(12/7)
            //===================================================================

            Product pod = new Product();

            ShopViewModel shopv = new ShopViewModel();

            List<CProductViewModel> list = new List<CProductViewModel>();

            List<CProductMainCategoryViewModel> cmcv = new List<CProductMainCategoryViewModel>();

            List<CProductSubCategoryViewModel> cscv = new List<CProductSubCategoryViewModel>();


            //List<CActivity> Activity = new List<CActivity>();
            //=====================================================================
            //取得團購商品(活動尚未結束)
            var ActivityProduct = from g in db.Activity.AsEnumerable()
                                  join p in db.Product.AsEnumerable()
                                  on g.ActivityID equals p.ActivityID
                                  where (g.EndTime >= DateTime.Now && p.Discontinued=="N")|| (g.EndTime <= DateTime.Now && p.Discontinued == "Y")
                                  select p;
        
            foreach (var item in ActivityProduct)
            {
              
               list.Add(new CProductViewModel() { entity = item });

            }
            //=====================================================================
            //分類商品
            var mainCategory = from k in db.ProductMainCategory
                               select k;

            foreach (var m in mainCategory)
            {
                cmcv.Add(new CProductMainCategoryViewModel { entity_MainCategory = m });
            }
            //=====================================================================
          
            shopv.MainCategory = cmcv;

            shopv.product = list;

           
            
            return View(shopv);
            
        }
        //=======================================================================

        #endregion
        public ActionResult test()
        {
            return View();
        }
        #region shop.cshtml
        public ActionResult shop()
        {
            var user = Session[CDictionary.welcome] as CMember;
            if (user == null) { return RedirectToAction("Login", "Member"); }
            SingleApartmentEntities db = new SingleApartmentEntities();
            ViewBag.MemberID = user.fMemberId;
            CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };
            var mymodel = theUser.SearchProduct();
            return View(mymodel);
        }
        #region 以圖搜關鍵字 & 價格搜尋
        [HttpPost]
        public async Task<ActionResult> shop(HttpPostedFileBase imgPhoto)
        {
            var user = Session[CDictionary.welcome] as CMember;
            if (user == null) { return RedirectToAction("Login", "Member"); }
            SingleApartmentEntities db = new SingleApartmentEntities();
            ViewBag.MemberID = user.fMemberId;
            CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };
            var result = theUser.SearchProduct();
             if (imgPhoto != null)
            {
                imgPhoto.SaveAs("c:\\temp\\111.jpg");
                FileStream fs = new FileStream("c:\\temp\\111.jpg", FileMode.Open, FileAccess.Read);
                int length = (int)fs.Length;
                byte[] image = new byte[length];
                fs.Read(image, 0, length);
                fs.Close();
                List<string> strs = await MakePredictionRequest(image);
                var list_product = theUser.SearchProductsBy(null, null, strs[0]); result.product = list_product;
                ViewBag.ByPhoto = "true";
                string Keyword = "";
                foreach (var str in strs)
                {
                    Keyword += (str + " ");
                }
                ViewBag.Keyword = Keyword;
            }
            return View(result);
        }
        public static async Task<List<string>> MakePredictionRequest(byte[] byteData)
        {
            var client = new HttpClient();
            // Request headers - replace this example key with your valid Prediction-Key.
            client.DefaultRequestHeaders.Add("Prediction-Key", "55f6001a7163417b95ab02f37900cf76");

            // Prediction URL - replace this example URL with your valid Prediction URL.
            string url = "https://sgapart-customvision.cognitiveservices.azure.com/customvision/v3.0/Prediction/fafe23de-ddc4-483c-acdc-ec5567cb35aa/classify/iterations/ProductModel/image";

            HttpResponseMessage response = new HttpResponseMessage();

            // Request body. Try this sample with a locally stored image.
            //byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
                var answer = await response.Content.ReadAsStringAsync();

                var test = JsonConvert.DeserializeObject<Answer>(answer);
                List<string> Myreturn = new List<string>(); ;
                foreach (var item in test.predictions)
                {
                    var ans = JsonConvert.SerializeObject(item);

                    var anstest = JsonConvert.DeserializeObject<MyObject>(ans);
                    if (anstest.probability > 0.9)
                        Myreturn.Add(anstest.tagName);
                }
                return Myreturn;
            }

        }
        #endregion
        public ActionResult PartialProductTabPane(string MemberID, int page = 1, string pageSize = "6", string MainCategory = null, string SubCategory = null, string KeyWord = "", string firstprice = "", string lastprice = "")
        {
            int currentPage = page < 1 ? 1 : page;
            CUser user = new CUser();
            List<CProductViewModel> lt;
            if(firstprice !="" && lastprice!= "")
                firstprice = firstprice.TrimStart('$'); lastprice = lastprice.TrimStart('$');
            if (KeyWord != "")
            {
                lt = user.SearchProductsBy(null, null, KeyWord);
            }
            else if (int.TryParse(firstprice, out int first) &&int.TryParse(lastprice, out int last) && last > first)
                lt = user.SearchProductsByPrice(first, last);
            else if (SubCategory != null)
                lt = user.SearchProductsBy(null, int.Parse(SubCategory));
            else if (MainCategory != null)
                lt = user.SearchProductsBy(int.Parse(MainCategory));
            else
                lt = user.SearchProductsBy();
            var result = lt.ToPagedList(currentPage, int.Parse(pageSize));
            ViewData.Model = result;
            ViewBag.MemberID = MemberID;
            ViewBag.pageSize = pageSize;
            return PartialView("_PartialProduct_TabPane");
        }
        public ActionResult PartialProductProfile(string MemberID, int page = 1, string pageSize = "6", string MainCategory = null, string SubCategory = null, string KeyWord = "", string firstprice = "", string lastprice = "")
        {
            int currentPage = page < 1 ? 1 : page;
            CUser user = new CUser();
            List<CProductViewModel> lt;
            if (firstprice != "" && lastprice != "")
                firstprice = firstprice.TrimStart('$'); lastprice = lastprice.TrimStart('$');
            if (KeyWord != "")
                lt = user.SearchProductsBy(null, null, KeyWord);
            else if (int.TryParse(firstprice, out int first) && int.TryParse(lastprice, out int last) && last > first)
                lt = user.SearchProductsByPrice(first, last);
            else if (SubCategory != null)
                lt = user.SearchProductsBy(null, int.Parse(SubCategory));
            else if (MainCategory != null)
                lt = user.SearchProductsBy(int.Parse(MainCategory));
            else
                lt = user.SearchProductsBy();
            var result = lt.ToPagedList(currentPage, int.Parse(pageSize));
            ViewData.Model = result;
            ViewBag.MemberID = MemberID;
            ViewBag.pageSize = pageSize;
            return PartialView("_PartialProduct_Profile");
        }

        #endregion
        #region FavoriteList.cshtml (1130改)
        public JsonResult AddToFavorite(string ProductID)
        {
            var user = Session[CDictionary.welcome] as CMember;
            SingleApartmentEntities db = new SingleApartmentEntities();
            var result = "發生錯誤，請重新登入再試一次！";
            if (user != null && int.TryParse(ProductID, out int pdID))
            {
                CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };
                result = theUser.AddToFavorite(pdID);
            }
            return Json(JsonConvert.SerializeObject(new { ans = result }));
        }

        public ActionResult Favoritelist()
        {
            var user = Session[CDictionary.welcome] as CMember;
            if (user == null) { return RedirectToAction("Login", "Member"); }
            SingleApartmentEntities db = new SingleApartmentEntities();
            ViewBag.MemberID = user.fMemberId;
            CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };
            var list = theUser.SearchFavorite();
            return View(list);
        }

        public JsonResult DeleteFavorite(string ProductID)
        {
            var user = Session[CDictionary.welcome] as CMember;
            SingleApartmentEntities db = new SingleApartmentEntities();
            var result = "發生錯誤，請重新登入再試一次！";
            if (user != null && int.TryParse(ProductID, out int pdID))
            {
                CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };
                result = theUser.DeleteFavorite(pdID);
            }
            return Json(JsonConvert.SerializeObject(new { ans = result }));
        }

        public ActionResult PartialFavorite(string MemberID, int page = 1, int pageSize = 6)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            var list = db.FavoriteList.Where(r => r.MemberID.ToString() == MemberID);
            int currentPage = page < 1 ? 1 : page;
            List<CFavoriteList> lt = new List<CFavoriteList>();
            foreach (var item in list)
            {
                lt.Add(new CFavoriteList { entity = item });
            }
            var result = lt.ToPagedList(currentPage, pageSize);
            ViewData.Model = result;
            ViewBag.MemberID = MemberID;
            return PartialView("_PartialFavorite");
        }
        #endregion
        #region 購物車
        //加到購物車，同步更新右上角的購物車區塊
        public ActionResult AddToCart(string ProductID = null, string Quantity = "1")
        {
            var list = Session[CDictionary.PRODUCTS_IN_CART] as List<CAddtoSessionView>;
            if (list == null)
            {
                list = new List<CAddtoSessionView>();
            }
            if (ProductID != null)
            {
                //如果購物車裡還沒有就Add、已經有了就加數量
                if (int.TryParse(ProductID, out int pdID))
                {
                    if (list.Where(r => r.txtProductID == pdID).Count() == 0)
                    {
                        list.Add(new CAddtoSessionView() { txtProductID = pdID, txtQuantity = int.Parse(Quantity) });
                    }
                    else
                    {
                        list.Where(r => r.txtProductID == pdID).FirstOrDefault().txtQuantity += int.Parse(Quantity);
                    }
                }
                Session[CDictionary.PRODUCTS_IN_CART] = list;
            }
            if (list.Count != 0)
            {
                ViewData.Model = (new CUser()).SearchProductInCart(list);
            }
            return PartialView("_PartialShoppingCart");
        }
        //顯示購物車內容
        public ActionResult ShowProductInCart()
        {
            var user = Session[CDictionary.welcome] as CMember;
            if (user == null) { return RedirectToAction("Login", "Member"); }

            SingleApartmentEntities db = new SingleApartmentEntities();
            ViewBag.MemberID = user.fMemberId;
            CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };
            List<CAddtoSessionView> list = Session[CDictionary.PRODUCTS_IN_CART] as List<CAddtoSessionView>;
            if (list != null && list.Count != 0)
                return View(theUser.SearchProductInCart(list));
            else
                return View();
        }
        //刪除購物車商品(一鍵清除)
        public ActionResult RemoveProductsInCart()
        {
            List<CAddtoSessionView> list = Session[CDictionary.PRODUCTS_IN_CART] as List<CAddtoSessionView>;
            if (list != null)
            {
                list = new List<CAddtoSessionView>();
                Session[CDictionary.PRODUCTS_IN_CART] = list;
            }
            return RedirectToAction("ShowProductInCart");
        }
        //刪除單一商品
        public JsonResult RemoveONEProductInCart(string ProductID)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            Product prod = db.Product.FirstOrDefault(p => p.ProductID.ToString() == ProductID);

            if (prod != null)
            {
                List<CAddtoSessionView> list = Session[CDictionary.PRODUCTS_IN_CART] as List<CAddtoSessionView>;
                if (list != null && list.Count != 0)
                {
                    for (int i = 0; i < list.Count; i++)     //foreach沒有辦法去修改自己本身的陣列
                    {
                        if (list[i].txtProductID.ToString() == ProductID)
                        {
                            list.Remove(list[i]);
                            return Json("成功");
                        }
                    }
                }
            }
            return Json("沒有此商品");
        }
        public JsonResult ChangeONEProductQuantity(string ProductID, string Quantity)
        {
            SingleApartmentEntities db = new SingleApartmentEntities();
            Product prod = db.Product.FirstOrDefault(p => p.ProductID.ToString() == ProductID);
            if (prod != null)
            {
                List<CAddtoSessionView> list = Session[CDictionary.PRODUCTS_IN_CART] as List<CAddtoSessionView>;
                if (list != null && list.Count != 0)
                {
                    int Sum = 0;
                    for (int i = 0; i < list.Count; i++)     //foreach沒有辦法去修改自己本身的陣列
                    {
                        if (list[i].txtProductID.ToString() == ProductID)
                        {
                            if (int.TryParse(Quantity, out int qty))
                            {
                                list[i].txtQuantity = qty;
                                Sum += list[i].txtQuantity * prod.UnitPrice;
                                
                            }
                        }
                        else
                        {
                            Sum += list[i].txtQuantity * db.Product.AsEnumerable().FirstOrDefault(p => p.ProductID == list[i].txtProductID).UnitPrice;
                        }
                    }
                    return Json(new { ans = "成功", sum = Sum });
                }
            }
            return Json(new{ans= "發生錯誤"});
        }

        #endregion
        #region Checkout

        //結帳畫面{12.6)
        public ActionResult CheckOut()
        {
            var user = Session[CDictionary.welcome] as CMember;
            if (user == null) { return RedirectToAction("Login", "Member"); }

            SingleApartmentEntities db = new SingleApartmentEntities();
            ViewBag.MemberID = user.fMemberId;
            CUser theUser = new CUser() { tMember = db.tMember.Where(r => r.fMemberId == user.fMemberId).FirstOrDefault() };
            List<CAddtoSessionView> list = Session[CDictionary.PRODUCTS_IN_CART] as List<CAddtoSessionView>;
            if (list == null || list.Count == 0)
            {
                return RedirectToAction("ShowProductInCart");
            }
            List<COrderDetailsViewModel> orderlist = theUser.SearchProductInCart(list);

            //CUser theUser = new CUser();
            ////===================================================================
            //var user = Session[CDictionary.welcome] as CMember;
            ////必須先登入會員 
            //if (user != null)
            //{
            //    user.fMemberName = Request.Form["TXTMEMBERNAME"];
            //    user.fPhone= Request.Form["TXTPHONE"];
            //    user.fEmail = Request.Form["TXTEMAIL"];
            //    //user.fBirthDate =Request.Form[""];
            //}
            //===================================================================

            return View(orderlist);
        }
        #endregion 
        //#region 秉庠


        ////===========================================================================
        ////傳回資料庫
        //[HttpPost]
        //public ActionResult ShowProductInCart(int MemberID)//第四步
        //{
        //    int totalPrice = 0;

        //    SingleApartmentEntities DB = new SingleApartmentEntities();

        //    List<COrderDetailsViewModel> list = Session[CDictionary.PRODUCTS_IN_CART] as List<COrderDetailsViewModel>;

        //    Order od = new Order();
        //    //抓出多筆資料
        //    foreach (var item in list)
        //    {
        //        OrderDetails ODD = new OrderDetails();

        //        ODD.ProductName = DB.Product.Where(p => p.ProductID == item.ProductID).FirstOrDefault().ProductName;

        //        ODD.Quantity = item.Quantity;

        //        ODD.ProductID = item.ProductID;

        //        ODD.UnitPrice = DB.Product.Where(p => p.ProductID == item.ProductID).FirstOrDefault().UnitPrice;

        //        totalPrice += item.Quantity * (DB.Product.Where(p => p.ProductID == item.ProductID).FirstOrDefault().UnitPrice);

        //        od.OrderDetails.Add(ODD);
        //    }

        //    //訂單日期
        //    od.OrderDate = DateTime.Now;
        //    //到貨日期
        //    od.ArrivedDate = DateTime.Now.AddDays(7);
        //    //總金額
        //    od.TotalAmount = totalPrice;

        //    od.OrderStatusID = 1;

        //    od.SendingStatus = "配送中";

        //    od.PayStatus = "尚未付款";

        //    od.MemberID = MemberID;//到時候要改成使用者的memberID


        //    DB.Order.Add(od);

        //    DB.SaveChanges();

        //    return RedirectToAction("Home");

        //}
        //訂單
        public ActionResult OrderList(int order_id = 0)
        {

            bool l_flag = false;  //顯示訂單明細
            SingleApartmentEntities db = new SingleApartmentEntities();

            int member_id = db.Order.FirstOrDefault().MemberID;


            IEnumerable<Order> l_order = from x in db.Order
                                         where x.MemberID > 0   //之後要改成memberID  先抓全部
                                         select x;


            List<COrder> list = new List<COrder>();
            foreach (Order o in l_order)
            {
                list.Add(new COrder() { order_entity = o });
            }

            IEnumerable<OrderDetails> l_orderdetail = from p in db.OrderDetails
                                                      where p.OrderID == order_id
                                                      select p;
            List<COrderDetails> odlist = new List<COrderDetails>();
            foreach (OrderDetails od in l_orderdetail)
            {
                var prod = db.Product.FirstOrDefault(x => x.ProductID == od.ProductID);
                odlist.Add(new COrderDetails() { entity = od, product_entity = prod });
            }

            COrderMasterDetail a = new COrderMasterDetail() { display_flag = l_flag, t_order = list, t_orderDetail = odlist };

            return View(a);

        }
        //訂單明細
        //public ActionResult List(int ID)
        //{
        //    using (SingleApartmentEntities db = new SingleApartmentEntities())
        //    {
        //        var table = (from p in db.OrderDetails
        //                     where p.OrderID == ID
        //                     select p).ToList();

        //}
        //取消訂單
        public ActionResult Delete(int id)
            {
                SingleApartmentEntities db = new SingleApartmentEntities();

                Order od = db.Order.FirstOrDefault(p => p.OrderID == id);

                var odd = db.OrderDetails.Where(q => q.OrderID == id);

                if (odd != null)
                {

                    foreach (var ITEM in odd)
                    {
                        db.OrderDetails.Remove(ITEM);

                    }
                    if (od != null)
                    {
                        db.Order.Remove(od);
                    }
                    db.SaveChanges();
                }

                return RedirectToAction("Home");
            }

        //#endregion



    }

} 

