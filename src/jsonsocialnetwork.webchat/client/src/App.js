import React from "react";
import Chat from "./Chat";
import LoginForm from "./LoginForm";

class App extends React.Component {
    constructor(props) {
        super(props);
        this.state = { user: undefined };
    }

    setUser = (user) => this.setState({ user: user })

    render() {
        const user = this.state.user;
        if (user === undefined) {
            return <LoginForm setUser={this.setUser} />
        }
        else {
            return <Chat user={user} setUser={this.setUser} />
        }
    }
}

export default App;