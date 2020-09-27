// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Net;
using HtmlAgilityPack;
using Humanizer;
using Ultz.Oppy.Properties;

namespace Ultz.Oppy.Content.Html
{
    /// <summary>
    /// Contains common HTML "mixins" (methods that modify HTML contents before being sent along the network)
    /// </summary>
    public static class HtmlMixins
    {
        /// <summary>
        /// Modifies the given HTML content using error page properties. Contains Oppy common mixins.
        /// </summary>
        /// <param name="content">The HTML content to modify.</param>
        /// <param name="code">The HTTP status code, used to determine the short error text.</param>
        /// <param name="longError">The long error text.</param>
        /// <returns>The modified HTML content.</returns>
        public static string ErrorPageMixin(string content, HttpStatusCode code, string? longError = null)
        {
            return ErrorPageMixin(content, (int) code + " " + code.Humanize(LetterCasing.Title), longError ??
                code switch
                {
                    HttpStatusCode.Continue => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.SwitchingProtocols => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.Processing => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.EarlyHints => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.OK => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.Created => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.Accepted => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.NonAuthoritativeInformation => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.NoContent => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.ResetContent => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.PartialContent => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.MultiStatus => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.AlreadyReported => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.IMUsed => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.Ambiguous => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.Moved => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.Found => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.RedirectMethod => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.NotModified => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.UseProxy => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.Unused => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.RedirectKeepVerb => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.PermanentRedirect => ErrorCodes.HandlerRanDry,
                    HttpStatusCode.BadRequest => ErrorCodes.BadRequest,
                    HttpStatusCode.Unauthorized => ErrorCodes.Unauthorized,
                    HttpStatusCode.PaymentRequired => ErrorCodes.PaymentRequired,
                    HttpStatusCode.Forbidden => ErrorCodes.Forbidden,
                    HttpStatusCode.NotFound => ErrorCodes.NotFound,
                    HttpStatusCode.MethodNotAllowed => ErrorCodes.MethodNotAllowed,
                    HttpStatusCode.NotAcceptable => ErrorCodes.NotAcceptable,
                    HttpStatusCode.ProxyAuthenticationRequired => ErrorCodes.ProxyAuthenticationRequired,
                    HttpStatusCode.RequestTimeout => ErrorCodes.RequestTimeout,
                    HttpStatusCode.Conflict => ErrorCodes.Conflict,
                    HttpStatusCode.Gone => ErrorCodes.Gone,
                    HttpStatusCode.LengthRequired => ErrorCodes.LengthRequired,
                    HttpStatusCode.PreconditionFailed => ErrorCodes.PreconditionFailed,
                    HttpStatusCode.RequestEntityTooLarge => ErrorCodes.RequestEntityTooLarge,
                    HttpStatusCode.RequestUriTooLong => ErrorCodes.RequestUriTooLong,
                    HttpStatusCode.UnsupportedMediaType => ErrorCodes.UnsupportedMediaType,
                    HttpStatusCode.RequestedRangeNotSatisfiable => ErrorCodes.RequestedRangeNotSatisfiable,
                    HttpStatusCode.ExpectationFailed => ErrorCodes.ExpectationFailed,
                    HttpStatusCode.MisdirectedRequest => ErrorCodes.MisdirectedRequest,
                    HttpStatusCode.UnprocessableEntity => ErrorCodes.UnprocessableEntity,
                    HttpStatusCode.Locked => ErrorCodes.Locked,
                    HttpStatusCode.FailedDependency => ErrorCodes.FailedDependency,
                    HttpStatusCode.UpgradeRequired => ErrorCodes.UpgradeRequired,
                    HttpStatusCode.PreconditionRequired => ErrorCodes.PreconditionRequired,
                    HttpStatusCode.TooManyRequests => ErrorCodes.TooManyRequests,
                    HttpStatusCode.RequestHeaderFieldsTooLarge => ErrorCodes.RequestHeaderFieldsTooLarge,
                    HttpStatusCode.UnavailableForLegalReasons => ErrorCodes.UnavailableForLegalReasons,
                    HttpStatusCode.InternalServerError => ErrorCodes.InternalServerError,
                    HttpStatusCode.NotImplemented => ErrorCodes.NotImplemented,
                    HttpStatusCode.BadGateway => ErrorCodes.BadGateway,
                    HttpStatusCode.ServiceUnavailable => ErrorCodes.ServiceUnavailable,
                    HttpStatusCode.GatewayTimeout => ErrorCodes.GatewayTimeout,
                    HttpStatusCode.HttpVersionNotSupported => ErrorCodes.HttpVersionNotSupported,
                    HttpStatusCode.VariantAlsoNegotiates => ErrorCodes.VariantAlsoNegotiates,
                    HttpStatusCode.InsufficientStorage => ErrorCodes.InsufficientStorage,
                    HttpStatusCode.LoopDetected => ErrorCodes.LoopDetected,
                    HttpStatusCode.NotExtended => ErrorCodes.NotExtended,
                    HttpStatusCode.NetworkAuthenticationRequired => ErrorCodes.NetworkAuthenticationRequired,
                    _ => ErrorCodes.UnknownError
                });
        }

        /// <summary>
        /// Modifies the given HTML content using error page properties. Contains Oppy common mixins.
        /// </summary>
        /// <param name="content">The HTML content to modify.</param>
        /// <param name="shortError">The short error text.</param>
        /// <param name="longError">The long error text.</param>
        /// <returns>The modified HTML content.</returns>
        public static string ErrorPageMixin(string content, string shortError, string longError)
        {
            longError = longError.Replace("\n", "<br />");
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            ApplyCommonMixins(doc);
            Mixin(doc, "oppy-short-error", shortError);
            Mixin(doc, "oppy-long-error", longError);
            return doc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// Modifies the given HTML content using common Oppy values, such as the current version and current year.
        /// </summary>
        /// <param name="doc">The HTML document to modify.</param>
        public static void ApplyCommonMixins(HtmlDocument doc)
        {
            Mixin(doc, "oppy-version", typeof(HtmlMixins).Assembly.GetName().Version?.ToString(3) ?? "X.Y.Z");
            Mixin(doc, "oppy-year", DateTime.Now.Year.ToString());
        }

        /// <summary>
        /// Mixes in (modifies) one descendant key, and replaces the <see cref="HtmlNode.InnerHtml" /> with the given
        /// value.
        /// </summary>
        /// <param name="doc">The document to modify.</param>
        /// <param name="key">The name of HTML elements to modify.</param>
        /// <param name="value">The new inner HTML value.</param>
        public static void Mixin(HtmlDocument doc, string key, string value)
        {
            foreach (var node in doc.DocumentNode.Descendants(key))
            {
                node.Name = "span";
                var attr = node.Attributes["id"];
                if (attr is null)
                {
                    node.Attributes.Add("id", key);
                }
                else
                {
                    attr.Value = key;
                }

                node.InnerHtml = value;
            }
        }
    }
}