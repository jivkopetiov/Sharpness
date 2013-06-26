using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sharpness
{
    // TODO
    // id - NSObject
    // SEL - ObjCRuntime.Selector
    //    [[UIImageView alloc] initWithFrame:
    //imageView.image
    //UIViewContentModeCenter
    //NSTextAlignmentCenter
    //closing ]
    //.opacity
    //[[UIScreen mainScreen] scale]
    //CGImageRef imageRef = CGImageCreateWithImageInRect([image CGImage], scaledRect);
    //UIImageOrientationUp
    //CGImageRelease(imageRef);
    //font pointSize
    // [aView isKindOfClass:self] -> aView is this.GetType()
    
    class Program
    {
        static void Main(string[] args)
        {
            new SharpnessParser().ParseFile(
@"C:\Users\jivko\Downloads\sharpness\tapkulibrary-master\tapkulibrary-master\src\TapkuLibrary\TKCoverflowView.m", @"C:\Users\Jivko\Downloads");
        }
    }
}
