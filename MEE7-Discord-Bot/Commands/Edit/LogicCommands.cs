using Discord;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    class LogicCommands : EditCommandProvider
    {
        public string GtDesc = "Greater than";
        public bool Gt(EditNull n, IMessage m, double a, double b) => a > b;

        public string LtDesc = "Less than";
        public bool Lt(EditNull n, IMessage m, double a, double b) => a < b;

        public string GeqDesc = "Greater than or equal";
        public bool Geq(EditNull n, IMessage m, double a, double b) => a >= b;

        public string LeqDesc = "Less than or equal";
        public bool Leq(EditNull n, IMessage m, double a, double b) => a <= b;

        public string EqDesc = "Equal";
        public bool Eq(EditNull n, IMessage m, double a, double b) => a == b;

        public string AndDesc = "And";
        public bool And(EditNull n, IMessage m, bool a, bool b) => a & b;

        public string OrDesc = "Or";
        public bool Or(EditNull n, IMessage m, bool a, bool b) => a | b;
    }
}
